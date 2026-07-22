using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PpvRecon.Api.Auth;
using PpvRecon.Api.Commands;
using PpvRecon.Api.Json;
using PpvRecon.Api.Middleware;
using PpvRecon.Api.Services;
using PpvRecon.Api.Services.BankStatement;
using PpvRecon.Api.Services.Settings;
using PpvRecon.Application.Auth;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Common;
using PpvRecon.Application.Jobs;
using PpvRecon.Application.Reconciliation;
using Serilog;
using PpvRecon.Infrastructure;
using PpvRecon.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var logFilePath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "..", "logs", "ppv-recon-.log"));

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(error => new ApiError
            {
                Field = x.Key,
                Message = string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "Dữ liệu không hợp lệ."
                    : error.ErrorMessage,
            }))
            .ToList();

        return new BadRequestObjectResult(ApiResponse<object?>.Fail("Dữ liệu không hợp lệ.", errors));
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditService, AuditService>();
// Cấu hình kết nối lấy dữ liệu đọc từ DB (qua IConnectionSettingsService) để Admin sửa áp dụng ngay.
// Timeout HttpClient điều khiển per-call theo cấu hình (CTS) nên để vô hạn tại đây.
builder.Services.AddScoped<IConnectionSettingsService, ConnectionSettingsService>();
builder.Services.AddHttpClient<IParkBalanceApiClient, ParkBalanceApiClient>(client =>
{
    client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
});
builder.Services.AddHttpClient<IOneInventoryBookingApiClient, OneInventoryBookingApiClient>(client =>
{
    client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
});
builder.Services.AddScoped<ITicketCostSyncService, TicketCostSyncService>();
builder.Services.AddScoped<IAgencyBookingSyncService, AgencyBookingSyncService>();
builder.Services.AddScoped<IImapEmailReader, ImapEmailReader>();
builder.Services.AddScoped<IBankStatementSyncService, BankStatementSyncService>();
builder.Services.AddScoped<IJobRunner, JobRunner>();
builder.Services.AddScoped<IReconciliationBuilder, ReconciliationBuilder>();
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IMaintenanceJobService, MaintenanceJobService>();
builder.Services.AddScoped<IDataResetService, DataResetService>();
builder.Services.AddHostedService<DailyScheduledJobHostedService>();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(ApiResponse<object?>.Fail("Bạn cần đăng nhập để tiếp tục."));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(ApiResponse<object?>.Fail("Bạn không có quyền thực hiện thao tác này."));
            },
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? ["http://localhost:5173", "http://127.0.0.1:5173"];

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRouting();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

if (!app.Environment.IsDevelopment())
{
    app.MapFallbackToFile("index.html");
}

try
{
    if (AdminSeedCommand.ShouldRun(args))
    {
        await AdminSeedCommand.RunAsync(app.Services);
        return;
    }

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<PpvReconDbContext>();

        // Tự động áp mọi migration đang chờ ngay khi khởi động, để DB luôn khớp với code.
        await db.Database.MigrateAsync();

        // Đảm bảo các cột cố định của "Quy trình nạp tiền KVC" luôn tồn tại
        // (tự phục hồi nếu DB từng bị xóa mất cột). Idempotent, an toàn mỗi lần chạy.
        await WorkflowBoardSeeder.EnsureColumnsAsync(db);
    }

    await app.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
