using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Api.Services.BankStatement;
using PpvRecon.Domain.Entities.Settings;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Services.Settings;

// ── DTO phục vụ màn "Cấu hình kết nối" ──

public sealed class MailConnectionDto
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 993;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Mailbox { get; set; } = "INBOX";
    public string FromFilter { get; set; } = "";
    public string PdfPassword { get; set; } = "";
}

public sealed class ParkBalanceConnectionDto
{
    public string Endpoint { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 30;
}

public sealed class OneInventoryConnectionDto
{
    public string BaseUrl { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 60;
    public string ParentAgencyCode { get; set; } = "5129";
}

public sealed class JobScheduleDto
{
    public string ParkBalanceTime { get; set; } = "23:59";
    public string TicketCostTime { get; set; } = "01:00";
    public string AgencyBookingTime { get; set; } = "23:59";
    public string BankScanStart { get; set; } = "04:00";
    public string BankScanEnd { get; set; } = "08:00";
    public int BankScanIntervalMinutes { get; set; } = 5;
}

public sealed class ConnectionSettingsDto
{
    public MailConnectionDto Mail { get; set; } = new();
    public ParkBalanceConnectionDto ParkBalance { get; set; } = new();
    public OneInventoryConnectionDto OneInventory { get; set; } = new();
    public JobScheduleDto JobSchedule { get; set; } = new();
}

public sealed class ConnectionTestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int DurationMs { get; set; }
}

/// <summary>Khung giờ chạy job, đã parse sẵn để scheduler dùng.</summary>
public sealed class JobScheduleSettings
{
    public TimeOnly ParkBalanceTime { get; set; } = new(23, 59);
    public TimeOnly TicketCostTime { get; set; } = new(1, 0);
    public TimeOnly AgencyBookingTime { get; set; } = new(23, 59);
    public TimeOnly BankScanStart { get; set; } = new(4, 0);
    public TimeOnly BankScanEnd { get; set; } = new(8, 0);
    public int BankScanIntervalMinutes { get; set; } = 5;
}

public interface IConnectionSettingsService
{
    // Getter cho các luồng lấy dữ liệu (đọc DB, fallback appsettings).
    Task<BankStatementImportOptions> GetBankStatementAsync(CancellationToken cancellationToken);
    Task<ParkBalanceApiOptions> GetParkBalanceAsync(CancellationToken cancellationToken);
    Task<OneInventoryApiOptions> GetOneInventoryAsync(CancellationToken cancellationToken);
    Task<JobScheduleSettings> GetJobScheduleAsync(CancellationToken cancellationToken);

    // Cho màn cấu hình.
    Task<ConnectionSettingsDto> GetAllAsync(CancellationToken cancellationToken);
    Task SaveAsync(ConnectionSettingsDto dto, int? userId, CancellationToken cancellationToken);

    Task<ConnectionTestResult> TestMailAsync(MailConnectionDto dto, CancellationToken cancellationToken);
    Task<ConnectionTestResult> TestParkBalanceAsync(ParkBalanceConnectionDto dto, CancellationToken cancellationToken);
    Task<ConnectionTestResult> TestOneInventoryAsync(OneInventoryConnectionDto dto, CancellationToken cancellationToken);
}

/// <summary>
/// Nguồn cấu hình kết nối lấy dữ liệu. Giá trị hiệu lực = override trong DB (SystemSettings,
/// prefix "Conn.") đè lên mặc định trong appsettings.json. Lưu vào DB nên Admin sửa xong áp dụng ngay.
/// </summary>
public sealed class ConnectionSettingsService(
    PpvReconDbContext dbContext,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory) : IConnectionSettingsService
{
    // ── Khóa lưu trong SystemSettings ──
    private const string MailHost = "Conn.BankStatement.Host";
    private const string MailPort = "Conn.BankStatement.Port";
    private const string MailUseSsl = "Conn.BankStatement.UseSsl";
    private const string MailUsername = "Conn.BankStatement.Username";
    private const string MailPassword = "Conn.BankStatement.Password";
    private const string MailMailbox = "Conn.BankStatement.Mailbox";
    private const string MailFromFilter = "Conn.BankStatement.FromFilter";
    private const string MailPdfPassword = "Conn.BankStatement.PdfPassword";

    private const string PbEndpoint = "Conn.ParkBalance.Endpoint";
    private const string PbTimeout = "Conn.ParkBalance.TimeoutSeconds";

    private const string OiBaseUrl = "Conn.OneInventory.BaseUrl";
    private const string OiUsername = "Conn.OneInventory.Username";
    private const string OiPassword = "Conn.OneInventory.Password";
    private const string OiTimeout = "Conn.OneInventory.TimeoutSeconds";
    private const string OiParentAgencyCode = "Conn.OneInventory.ParentAgencyCode";

    private const string JobParkBalanceTime = "Conn.Jobs.ParkBalanceTime";
    private const string JobTicketCostTime = "Conn.Jobs.TicketCostTime";
    private const string JobAgencyBookingTime = "Conn.Jobs.AgencyBookingTime";
    private const string JobBankScanStart = "Conn.Jobs.BankScanStart";
    private const string JobBankScanEnd = "Conn.Jobs.BankScanEnd";
    private const string JobBankScanInterval = "Conn.Jobs.BankScanIntervalMinutes";

    private async Task<Dictionary<string, string>> LoadOverridesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SystemSettings
            .AsNoTracking()
            .Where(x => x.Key.StartsWith("Conn."))
            .ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken);
    }

    public async Task<BankStatementImportOptions> GetBankStatementAsync(CancellationToken cancellationToken)
    {
        var def = configuration.GetSection("BankStatementImport").Get<BankStatementImportOptions>() ?? new();
        var o = await LoadOverridesAsync(cancellationToken);
        return new BankStatementImportOptions
        {
            Host = Str(o, MailHost, def.Host),
            Port = IntVal(o, MailPort, def.Port),
            UseSsl = Bool(o, MailUseSsl, def.UseSsl),
            Username = Str(o, MailUsername, def.Username),
            Password = Str(o, MailPassword, def.Password),
            Mailbox = Str(o, MailMailbox, def.Mailbox),
            FromFilter = Str(o, MailFromFilter, def.FromFilter),
            PdfPassword = Str(o, MailPdfPassword, def.PdfPassword),
        };
    }

    public async Task<ParkBalanceApiOptions> GetParkBalanceAsync(CancellationToken cancellationToken)
    {
        var def = configuration.GetSection("ExternalApis:ParkBalance").Get<ParkBalanceApiOptions>() ?? new();
        var o = await LoadOverridesAsync(cancellationToken);
        return new ParkBalanceApiOptions
        {
            Endpoint = Str(o, PbEndpoint, def.Endpoint),
            TimeoutSeconds = IntVal(o, PbTimeout, def.TimeoutSeconds),
        };
    }

    public async Task<OneInventoryApiOptions> GetOneInventoryAsync(CancellationToken cancellationToken)
    {
        var def = configuration.GetSection("ExternalApis:OneInventory").Get<OneInventoryApiOptions>() ?? new();
        var o = await LoadOverridesAsync(cancellationToken);
        return new OneInventoryApiOptions
        {
            BaseUrl = Str(o, OiBaseUrl, def.BaseUrl),
            Username = Str(o, OiUsername, def.Username),
            Password = Str(o, OiPassword, def.Password),
            TimeoutSeconds = IntVal(o, OiTimeout, def.TimeoutSeconds),
            ParentAgencyCode = Str(o, OiParentAgencyCode,
                string.IsNullOrWhiteSpace(def.ParentAgencyCode) ? "5129" : def.ParentAgencyCode),
        };
    }

    public async Task<JobScheduleSettings> GetJobScheduleAsync(CancellationToken cancellationToken)
    {
        var o = await LoadOverridesAsync(cancellationToken);
        return new JobScheduleSettings
        {
            ParkBalanceTime = Time(o, JobParkBalanceTime, configuration["Jobs:ScheduleTimes:ParkBalance"], new(23, 59)),
            TicketCostTime = Time(o, JobTicketCostTime, configuration["Jobs:ScheduleTimes:TicketCost"], new(1, 0)),
            AgencyBookingTime = Time(o, JobAgencyBookingTime, configuration["Jobs:ScheduleTimes:AgencyBooking"], new(23, 59)),
            BankScanStart = Time(o, JobBankScanStart, configuration["Jobs:BankTransactionScan:StartTime"], new(4, 0)),
            BankScanEnd = Time(o, JobBankScanEnd, configuration["Jobs:BankTransactionScan:EndTime"], new(8, 0)),
            BankScanIntervalMinutes = Math.Clamp(
                IntVal(o, JobBankScanInterval, configuration.GetValue("Jobs:BankTransactionScan:IntervalMinutes", 5)),
                1, 720),
        };
    }

    public async Task<ConnectionSettingsDto> GetAllAsync(CancellationToken cancellationToken)
    {
        var mail = await GetBankStatementAsync(cancellationToken);
        var pb = await GetParkBalanceAsync(cancellationToken);
        var oi = await GetOneInventoryAsync(cancellationToken);
        var job = await GetJobScheduleAsync(cancellationToken);

        return new ConnectionSettingsDto
        {
            Mail = new MailConnectionDto
            {
                Host = mail.Host,
                Port = mail.Port,
                UseSsl = mail.UseSsl,
                Username = mail.Username,
                Password = mail.Password,
                Mailbox = mail.Mailbox,
                FromFilter = mail.FromFilter,
                PdfPassword = mail.PdfPassword,
            },
            ParkBalance = new ParkBalanceConnectionDto
            {
                Endpoint = pb.Endpoint,
                TimeoutSeconds = pb.TimeoutSeconds,
            },
            OneInventory = new OneInventoryConnectionDto
            {
                BaseUrl = oi.BaseUrl,
                Username = oi.Username,
                Password = oi.Password,
                TimeoutSeconds = oi.TimeoutSeconds,
                ParentAgencyCode = oi.ParentAgencyCode,
            },
            JobSchedule = new JobScheduleDto
            {
                ParkBalanceTime = job.ParkBalanceTime.ToString("HH:mm"),
                TicketCostTime = job.TicketCostTime.ToString("HH:mm"),
                AgencyBookingTime = job.AgencyBookingTime.ToString("HH:mm"),
                BankScanStart = job.BankScanStart.ToString("HH:mm"),
                BankScanEnd = job.BankScanEnd.ToString("HH:mm"),
                BankScanIntervalMinutes = job.BankScanIntervalMinutes,
            },
        };
    }

    public async Task SaveAsync(ConnectionSettingsDto dto, int? userId, CancellationToken cancellationToken)
    {
        var existing = await dbContext.SystemSettings
            .Where(x => x.Key.StartsWith("Conn."))
            .ToDictionaryAsync(x => x.Key, x => x, cancellationToken);

        var nowUtc = DateTime.UtcNow;

        void Set(string key, string value, SettingValueType type, bool sensitive)
        {
            if (existing.TryGetValue(key, out var row))
            {
                row.Value = value;
                row.ValueType = type;
                row.IsSensitive = sensitive;
                row.UpdatedAtUtc = nowUtc;
                row.UpdatedByUserId = userId;
            }
            else
            {
                dbContext.SystemSettings.Add(new SystemSetting
                {
                    Key = key,
                    Value = value,
                    ValueType = type,
                    IsSensitive = sensitive,
                    Description = "Cấu hình kết nối (sửa qua màn Cấu hình kết nối).",
                    CreatedAtUtc = nowUtc,
                    UpdatedAtUtc = nowUtc,
                    UpdatedByUserId = userId,
                });
            }
        }

        var port = Math.Clamp(dto.Mail.Port, 1, 65535);
        Set(MailHost, dto.Mail.Host.Trim(), SettingValueType.String, false);
        Set(MailPort, port.ToString(), SettingValueType.Int, false);
        Set(MailUseSsl, dto.Mail.UseSsl ? "true" : "false", SettingValueType.Bool, false);
        Set(MailUsername, dto.Mail.Username.Trim(), SettingValueType.String, false);
        Set(MailPassword, dto.Mail.Password, SettingValueType.String, true);
        Set(MailMailbox, string.IsNullOrWhiteSpace(dto.Mail.Mailbox) ? "INBOX" : dto.Mail.Mailbox.Trim(), SettingValueType.String, false);
        Set(MailFromFilter, dto.Mail.FromFilter.Trim(), SettingValueType.String, false);
        Set(MailPdfPassword, dto.Mail.PdfPassword, SettingValueType.String, true);

        Set(PbEndpoint, dto.ParkBalance.Endpoint.Trim(), SettingValueType.String, false);
        Set(PbTimeout, Math.Clamp(dto.ParkBalance.TimeoutSeconds, 1, 300).ToString(), SettingValueType.Int, false);

        Set(OiBaseUrl, dto.OneInventory.BaseUrl.Trim(), SettingValueType.String, false);
        Set(OiUsername, dto.OneInventory.Username.Trim(), SettingValueType.String, false);
        Set(OiPassword, dto.OneInventory.Password, SettingValueType.String, true);
        Set(OiTimeout, Math.Clamp(dto.OneInventory.TimeoutSeconds, 1, 300).ToString(), SettingValueType.Int, false);
        Set(OiParentAgencyCode,
            string.IsNullOrWhiteSpace(dto.OneInventory.ParentAgencyCode) ? "5129" : dto.OneInventory.ParentAgencyCode.Trim(),
            SettingValueType.String, false);

        Set(JobParkBalanceTime, NormalizeTime(dto.JobSchedule.ParkBalanceTime, new(23, 59)), SettingValueType.String, false);
        Set(JobTicketCostTime, NormalizeTime(dto.JobSchedule.TicketCostTime, new(1, 0)), SettingValueType.String, false);
        Set(JobAgencyBookingTime, NormalizeTime(dto.JobSchedule.AgencyBookingTime, new(23, 59)), SettingValueType.String, false);
        Set(JobBankScanStart, NormalizeTime(dto.JobSchedule.BankScanStart, new(4, 0)), SettingValueType.String, false);
        Set(JobBankScanEnd, NormalizeTime(dto.JobSchedule.BankScanEnd, new(8, 0)), SettingValueType.String, false);
        Set(JobBankScanInterval, Math.Clamp(dto.JobSchedule.BankScanIntervalMinutes, 1, 720).ToString(), SettingValueType.Int, false);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // ── Kiểm tra kết nối ──

    public async Task<ConnectionTestResult> TestMailAsync(MailConnectionDto dto, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var client = new ImapClient();
            var socketOptions = dto.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
            await client.ConnectAsync(dto.Host, dto.Port, socketOptions, cancellationToken);
            await client.AuthenticateAsync(dto.Username, dto.Password, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            sw.Stop();
            return Ok(sw, "Kết nối và đăng nhập IMAP thành công.");
        }
        catch (Exception ex)
        {
            sw.Stop();
            return Fail(sw, $"Không kết nối được hộp thư: {ex.Message}");
        }
    }

    public async Task<ConnectionTestResult> TestParkBalanceAsync(ParkBalanceConnectionDto dto, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var endpoint = dto.Endpoint?.Trim();
        if (string.IsNullOrWhiteSpace(endpoint) || !Uri.TryCreate(endpoint, UriKind.Absolute, out _))
        {
            return Fail(sw, "Endpoint không hợp lệ.");
        }

        try
        {
            using var http = httpClientFactory.CreateClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(dto.TimeoutSeconds, 1, 300)));

            // Gọi thử endpoint (payload tối thiểu). Có phản hồi HTTP = kết nối được, kể cả khi nội dung lỗi.
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent("{\"value\":{}}", Encoding.UTF8, "application/json"),
            };
            using var response = await http.SendAsync(request, cts.Token);
            sw.Stop();
            return Ok(sw, $"Kết nối được tới endpoint (HTTP {(int)response.StatusCode}).");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            sw.Stop();
            return Fail(sw, "Gọi endpoint quá thời gian chờ.");
        }
        catch (Exception ex)
        {
            sw.Stop();
            return Fail(sw, $"Không gọi được endpoint: {ex.Message}");
        }
    }

    public async Task<ConnectionTestResult> TestOneInventoryAsync(OneInventoryConnectionDto dto, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var baseUrl = (string.IsNullOrWhiteSpace(dto.BaseUrl) ? "" : dto.BaseUrl).TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
        {
            return Fail(sw, "BaseUrl không hợp lệ.");
        }
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return Fail(sw, "Chưa nhập username/password.");
        }

        try
        {
            using var http = httpClientFactory.CreateClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(dto.TimeoutSeconds, 1, 300)));

            var loginUrl = $"{baseUrl}/api/v1/admin/user/login";
            var payloadJson = JsonSerializer.Serialize(new { username = dto.Username, password = dto.Password });
            using var request = new HttpRequestMessage(HttpMethod.Post, loginUrl)
            {
                Content = new StringContent(payloadJson, Encoding.UTF8, "application/json"),
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await http.SendAsync(request, cts.Token);
            var body = await response.Content.ReadAsStringAsync(cts.Token);
            sw.Stop();

            if (!response.IsSuccessStatusCode)
            {
                return Fail(sw, $"Đăng nhập thất bại (HTTP {(int)response.StatusCode}).");
            }

            using var doc = JsonDocument.Parse(body);
            var hasToken = doc.RootElement.TryGetProperty("value", out var value)
                && value.ValueKind == JsonValueKind.Object
                && value.TryGetProperty("token", out var token)
                && token.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(token.GetString());

            return hasToken
                ? Ok(sw, "Đăng nhập OneInventory thành công, đã lấy được token.")
                : Fail(sw, "Đăng nhập nhưng không lấy được token (sai tài khoản hoặc API đổi cấu trúc).");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            sw.Stop();
            return Fail(sw, "Gọi API OneInventory quá thời gian chờ.");
        }
        catch (Exception ex)
        {
            sw.Stop();
            return Fail(sw, $"Không gọi được OneInventory: {ex.Message}");
        }
    }

    // ── Helpers ──

    private static ConnectionTestResult Ok(Stopwatch sw, string message)
        => new() { Success = true, Message = message, DurationMs = (int)sw.ElapsedMilliseconds };

    private static ConnectionTestResult Fail(Stopwatch sw, string message)
        => new() { Success = false, Message = message, DurationMs = (int)sw.ElapsedMilliseconds };

    private static string Str(IReadOnlyDictionary<string, string> o, string key, string fallback)
        => o.TryGetValue(key, out var v) ? v : fallback;

    private static int IntVal(IReadOnlyDictionary<string, string> o, string key, int fallback)
        => o.TryGetValue(key, out var v) && int.TryParse(v, out var parsed) ? parsed : fallback;

    private static bool Bool(IReadOnlyDictionary<string, string> o, string key, bool fallback)
        => o.TryGetValue(key, out var v) && bool.TryParse(v, out var parsed) ? parsed : fallback;

    private static TimeOnly Time(IReadOnlyDictionary<string, string> o, string key, string? appSettingsValue, TimeOnly fallback)
    {
        if (o.TryGetValue(key, out var v) && TimeOnly.TryParse(v, out var parsed)) return parsed;
        if (TimeOnly.TryParse(appSettingsValue, out var fromConfig)) return fromConfig;
        return fallback;
    }

    private static string NormalizeTime(string value, TimeOnly fallback)
        => (TimeOnly.TryParse(value, out var parsed) ? parsed : fallback).ToString("HH:mm");
}
