using Microsoft.EntityFrameworkCore;
using PpvRecon.Api.Services.Settings;
using PpvRecon.Application.Jobs;
using PpvRecon.Application.Reconciliation;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Services;

public sealed class DailyScheduledJobHostedService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<DailyScheduledJobHostedService> logger) : BackgroundService
{
    // Các nguồn chạy 1 lần/ngày theo mốc giờ riêng (BankTransaction tách riêng vì quét lặp 4h–8h).
    private static readonly ExternalApiSource[] SyncSources =
    [
        ExternalApiSource.ParkBalance,
        ExternalApiSource.TicketCost,
        ExternalApiSource.AgencyBooking,
        ExternalApiSource.ArTransaction,
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollSeconds = Math.Clamp(configuration.GetValue("Jobs:SchedulerPollSeconds", 30), 10, 3600);
        var pollDelay = TimeSpan.FromSeconds(pollSeconds);

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (configuration.GetValue("Jobs:SchedulerEnabled", true))
                {
                    await RunIfDueAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scheduled daily job workflow failed.");
            }

            try
            {
                await Task.Delay(pollDelay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task RunIfDueAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PpvReconDbContext>();

        var timeZone = GetVietnamTimeZone();
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        var nowTime = TimeOnly.FromDateTime(localNow);
        var businessDate = DateOnly.FromDateTime(localNow);

        var jobRunner = scope.ServiceProvider.GetRequiredService<IJobRunner>();
        var reconciliationBuilder = scope.ServiceProvider.GetRequiredService<IReconciliationBuilder>();
        var maintenanceJobService = scope.ServiceProvider.GetRequiredService<IMaintenanceJobService>();
        var connectionSettings = scope.ServiceProvider.GetRequiredService<IConnectionSettingsService>();

        // Khung giờ chạy job (Admin sửa qua màn "Cấu hình kết nối", áp dụng ngay).
        var schedule = await connectionSettings.GetJobScheduleAsync(cancellationToken);

        // 1) Các job đồng bộ API ngoài: mỗi nguồn chạy theo khung giờ riêng.
        // Số dư KVC chốt lúc cuối ngày T (23:59) cho chính ngày T; giá vốn vé chạy rạng sáng
        // hôm sau (mặc định 01:00) nhưng lấy dữ liệu của NGÀY HÔM TRƯỚC để trọn vẹn cả ngày bán vé;
        // giao dịch đại lý trên TA chốt 23:59 và lấy CHÍNH NGÀY T (theo phương án đã chốt).
        foreach (var source in SyncSources)
        {
            var sourceTime = source switch
            {
                ExternalApiSource.ParkBalance => schedule.ParkBalanceTime,
                ExternalApiSource.TicketCost => schedule.TicketCostTime,
                ExternalApiSource.AgencyBooking => schedule.AgencyBookingTime,
                ExternalApiSource.ArTransaction => schedule.ArTransactionTime,
                _ => schedule.ParkBalanceTime,
            };
            if (nowTime < sourceTime)
            {
                continue;
            }

            var targetDate = source == ExternalApiSource.TicketCost
                ? businessDate.AddDays(-1)
                : businessDate;

            var jobName = GetJobName(source);
            if (await HasScheduledJobAsync(dbContext, jobName, targetDate, cancellationToken))
            {
                continue;
            }

            logger.LogInformation(
                "Running scheduled sync {JobName} for business date {BusinessDate}.",
                jobName,
                targetDate);

            await jobRunner.RunExternalSyncAsync(
                source,
                targetDate,
                JobTriggerType.Schedule,
                triggeredByUserId: null,
                cancellationToken);
        }

        // 1b) Quét sao kê BIDV từ email: chạy lặp mỗi N phút trong khung 4h–8h sáng (giờ VN).
        await RunBankTransactionScanIfDueAsync(dbContext, jobRunner, schedule, nowTime, businessDate, cancellationToken);

        // 2) Đối soát + tổng hợp lỗi + dọn log: dùng mốc giờ chung (DB SystemSettings "Jobs.ScheduleTime").
        var sharedTime = await GetScheduleTimeAsync(dbContext, cancellationToken);
        if (nowTime < sharedTime)
        {
            return;
        }

        if (!await HasScheduledJobAsync(dbContext, "BuildParkReconciliation", businessDate, cancellationToken))
        {
            await reconciliationBuilder.BuildAsync(
                businessDate,
                triggeredByUserId: null,
                cancellationToken,
                JobTriggerType.Schedule);
        }

        if (!await HasScheduledJobAsync(dbContext, "SendDailySyncErrorSummary", businessDate, cancellationToken))
        {
            await maintenanceJobService.SendSyncErrorSummaryAsync(
                businessDate,
                triggeredByUserId: null,
                cancellationToken,
                JobTriggerType.Schedule);
        }

        if (!await HasCleanupRunTodayAsync(dbContext, localNow, timeZone, cancellationToken))
        {
            await maintenanceJobService.CleanupAuditLogsAsync(
                triggeredByUserId: null,
                cancellationToken,
                JobTriggerType.Schedule);
        }
    }

    /// <summary>
    /// Quét sao kê BIDV từ email theo kiểu lặp: chỉ chạy trong khung giờ [Start, End) (giờ VN),
    /// và đảm bảo mỗi lần quét cách nhau ít nhất IntervalMinutes phút. Khung giờ lấy từ cấu hình kết nối.
    /// </summary>
    private async Task RunBankTransactionScanIfDueAsync(
        PpvReconDbContext dbContext,
        IJobRunner jobRunner,
        JobScheduleSettings schedule,
        TimeOnly nowTime,
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        // Ngoài khung giờ 4h–8h thì không quét.
        if (nowTime < schedule.BankScanStart || nowTime >= schedule.BankScanEnd)
        {
            return;
        }

        var intervalMinutes = schedule.BankScanIntervalMinutes;

        // Phải cách lần quét gần nhất trong ngày >= IntervalMinutes phút.
        var jobName = GetJobName(ExternalApiSource.BankTransaction);
        var lastStartedAtUtc = await dbContext.JobRuns
            .Where(x => x.JobName == jobName
                && x.BusinessDate == businessDate
                && x.TriggeredBy == JobTriggerType.Schedule)
            .OrderByDescending(x => x.StartedAtUtc)
            .Select(x => (DateTime?)x.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastStartedAtUtc is not null
            && DateTime.UtcNow - lastStartedAtUtc.Value < TimeSpan.FromMinutes(intervalMinutes))
        {
            return;
        }

        logger.LogInformation(
            "Running scheduled bank statement scan {JobName} for business date {BusinessDate} at {LocalTime}.",
            jobName,
            businessDate,
            nowTime);

        await jobRunner.RunExternalSyncAsync(
            ExternalApiSource.BankTransaction,
            businessDate,
            JobTriggerType.Schedule,
            triggeredByUserId: null,
            cancellationToken);
    }

    private static async Task<TimeOnly> GetScheduleTimeAsync(PpvReconDbContext dbContext, CancellationToken cancellationToken)
    {
        var configuredValue = await dbContext.SystemSettings
            .AsNoTracking()
            .Where(x => x.Key == "Jobs.ScheduleTime")
            .Select(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return TimeOnly.TryParse(configuredValue, out var parsed)
            ? parsed
            : new TimeOnly(23, 59);
    }

    private static Task<bool> HasScheduledJobAsync(
        PpvReconDbContext dbContext,
        string jobName,
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        return dbContext.JobRuns.AnyAsync(
            x => x.JobName == jobName
                && x.BusinessDate == businessDate
                && x.TriggeredBy == JobTriggerType.Schedule,
            cancellationToken);
    }

    private static Task<bool> HasCleanupRunTodayAsync(
        PpvReconDbContext dbContext,
        DateTime localNow,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken)
    {
        var localStart = localNow.Date;
        var localEnd = localStart.AddDays(1);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(localStart, timeZone);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(localEnd, timeZone);

        return dbContext.JobRuns.AnyAsync(
            x => x.JobName == "CleanupAuditLogs"
                && x.TriggeredBy == JobTriggerType.Schedule
                && x.StartedAtUtc >= startUtc
                && x.StartedAtUtc < endUtc,
            cancellationToken);
    }

    private static TimeZoneInfo GetVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
        }
    }

    private static string GetJobName(ExternalApiSource source)
    {
        return source switch
        {
            ExternalApiSource.ParkBalance => "SyncParkBalances",
            ExternalApiSource.TicketCost => "SyncTicketCosts",
            ExternalApiSource.BankTransaction => "SyncBankTransactions",
            ExternalApiSource.AgencyBooking => "SyncAgencyBookings",
            ExternalApiSource.ArTransaction => "SyncArTransactions",
            _ => $"Sync{source}",
        };
    }
}
