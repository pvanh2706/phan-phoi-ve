using Microsoft.EntityFrameworkCore;
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
    private static readonly ExternalApiSource[] SyncSources =
    [
        ExternalApiSource.ParkBalance,
        ExternalApiSource.TicketCost,
        ExternalApiSource.BankTransaction,
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
        var scheduleTime = await GetScheduleTimeAsync(dbContext, cancellationToken);

        if (TimeOnly.FromDateTime(localNow) < scheduleTime)
        {
            return;
        }

        var businessDate = DateOnly.FromDateTime(localNow);
        var completedSummary = await HasScheduledJobAsync(
            dbContext,
            "SendDailySyncErrorSummary",
            businessDate,
            cancellationToken);

        if (completedSummary && await HasCleanupRunTodayAsync(dbContext, localNow, timeZone, cancellationToken))
        {
            return;
        }

        logger.LogInformation("Running scheduled daily workflow for business date {BusinessDate}.", businessDate);

        var jobRunner = scope.ServiceProvider.GetRequiredService<IJobRunner>();
        var reconciliationBuilder = scope.ServiceProvider.GetRequiredService<IReconciliationBuilder>();
        var maintenanceJobService = scope.ServiceProvider.GetRequiredService<IMaintenanceJobService>();

        foreach (var source in SyncSources)
        {
            var jobName = GetJobName(source);
            if (await HasScheduledJobAsync(dbContext, jobName, businessDate, cancellationToken))
            {
                continue;
            }

            await jobRunner.RunExternalSyncAsync(
                source,
                businessDate,
                JobTriggerType.Schedule,
                triggeredByUserId: null,
                cancellationToken);
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
            _ => $"Sync{source}",
        };
    }
}
