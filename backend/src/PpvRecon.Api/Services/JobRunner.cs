using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Jobs;
using PpvRecon.Domain.Entities.Jobs;
using PpvRecon.Domain.Entities.Parks;
using PpvRecon.Domain.Entities.Summaries;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Services;

public sealed class JobRunner(
    PpvReconDbContext dbContext,
    IParkBalanceApiClient parkBalanceApiClient,
    IAuditService auditService) : IJobRunner
{
    public Task<JobRunDetailDto> RunExternalSyncAsync(
        ExternalApiSource source,
        DateOnly businessDate,
        JobTriggerType triggeredBy,
        int? triggeredByUserId,
        CancellationToken cancellationToken = default)
    {
        return source == ExternalApiSource.ParkBalance
            ? RunParkBalanceSyncAsync(source, businessDate, triggeredBy, triggeredByUserId, cancellationToken)
            : RunExternalSyncPlaceholderAsync(source, businessDate, triggeredBy, triggeredByUserId, cancellationToken);
    }

    private async Task<JobRunDetailDto> RunParkBalanceSyncAsync(
        ExternalApiSource source,
        DateOnly businessDate,
        JobTriggerType triggeredBy,
        int? triggeredByUserId,
        CancellationToken cancellationToken)
    {
        var jobRun = new JobRun
        {
            JobName = GetJobName(source),
            BusinessDate = businessDate,
            TriggeredBy = triggeredBy,
            TriggeredByUserId = triggeredByUserId,
            StartedAtUtc = DateTime.UtcNow,
            Status = JobRunStatus.Running,
        };

        dbContext.JobRuns.Add(jobRun);
        await dbContext.SaveChangesAsync(cancellationToken);

        var parks = await dbContext.Parks
            .Where(x => !x.IsDeleted && x.Status == RecordStatus.Active)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        jobRun.TotalItems = parks.Count;
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var park in parks)
        {
            var itemStartedAtUtc = DateTime.UtcNow;
            var item = new JobRunItem
            {
                JobRunId = jobRun.Id,
                BusinessDate = businessDate,
                ParkId = park.Id,
                Source = source,
                Status = JobRunItemStatus.Running,
                AttemptCount = 1,
                StartedAtUtc = itemStartedAtUtc,
            };

            dbContext.JobRunItems.Add(item);
            await dbContext.SaveChangesAsync(cancellationToken);

            var apiResult = await parkBalanceApiClient.FetchAsync(park, businessDate, cancellationToken);
            var finishedAtUtc = DateTime.UtcNow;
            var rawResponse = new ExternalApiRawResponse
            {
                Source = source,
                BusinessDate = businessDate,
                ParkId = park.Id,
                JobRunId = jobRun.Id,
                JobRunItemId = item.Id,
                RequestUrl = apiResult.RequestUrl,
                RequestPayloadJson = apiResult.RequestPayloadJson,
                ResponseStatusCode = apiResult.ResponseStatusCode,
                ResponseBodyJson = apiResult.ResponseBodyJson,
                IsSuccess = apiResult.IsSuccess,
                ErrorMessage = apiResult.ErrorMessage,
                DurationMs = apiResult.DurationMs,
                ReceivedAtUtc = finishedAtUtc,
            };

            dbContext.ExternalApiRawResponses.Add(rawResponse);
            await dbContext.SaveChangesAsync(cancellationToken);

            item.RawResponseId = rawResponse.Id;
            item.FinishedAtUtc = finishedAtUtc;
            item.DurationMs = apiResult.DurationMs;

            if (apiResult.IsSuccess && apiResult.AvailableBalance is not null)
            {
                item.Status = JobRunItemStatus.Succeeded;
                await UpsertParkBalanceSnapshotAsync(
                    park,
                    businessDate,
                    apiResult.AvailableBalance.Value,
                    jobRun.Id,
                    item.Id,
                    rawResponse.Id,
                    triggeredByUserId,
                    finishedAtUtc,
                    cancellationToken);
                jobRun.SuccessItems++;
            }
            else
            {
                item.Status = JobRunItemStatus.Failed;
                item.ErrorCode = apiResult.ErrorCode;
                item.ErrorMessage = apiResult.ErrorMessage;
                jobRun.FailedItems++;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        jobRun.FinishedAtUtc = DateTime.UtcNow;
        jobRun.SkippedItems = 0;
        jobRun.Status = jobRun.FailedItems == 0 ? JobRunStatus.Succeeded : JobRunStatus.CompletedWithErrors;
        jobRun.ErrorMessage = jobRun.FailedItems == 0
            ? null
            : $"Có {jobRun.FailedItems}/{jobRun.TotalItems} KVC không lấy được số dư.";
        jobRun.SummaryJson = JsonSerializer.Serialize(new
        {
            source = source.ToString(),
            businessDate,
            activeParkCount = parks.Count,
            successItems = jobRun.SuccessItems,
            failedItems = jobRun.FailedItems,
            externalApiConfigured = true,
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await LogJobRunAsync(jobRun, triggeredByUserId, cancellationToken);

        return await BuildJobRunDetailAsync(jobRun.Id, cancellationToken);
    }

    private async Task UpsertParkBalanceSnapshotAsync(
        Park park,
        DateOnly businessDate,
        long availableBalance,
        int jobRunId,
        int jobRunItemId,
        int rawResponseId,
        int? triggeredByUserId,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var snapshot = await dbContext.DailyParkBalanceSnapshots
            .FirstOrDefaultAsync(x => x.BusinessDate == businessDate && x.ParkId == park.Id, cancellationToken);

        if (snapshot is null)
        {
            snapshot = new DailyParkBalanceSnapshot
            {
                BusinessDate = businessDate,
                ParkId = park.Id,
                CreatedAtUtc = nowUtc,
                CreatedByUserId = triggeredByUserId,
            };
            dbContext.DailyParkBalanceSnapshots.Add(snapshot);
        }
        else
        {
            snapshot.UpdatedAtUtc = nowUtc;
            snapshot.UpdatedByUserId = triggeredByUserId;
        }

        snapshot.PaymentType = park.PaymentType;
        snapshot.AvailableBalance = availableBalance;
        snapshot.BankAccountSnapshot = park.BankAccount;
        snapshot.SourceType = SourceType.Api;
        snapshot.SourceJobRunId = jobRunId;
        snapshot.SourceJobRunItemId = jobRunItemId;
        snapshot.RawResponseId = rawResponseId;
    }

    private async Task<JobRunDetailDto> RunExternalSyncPlaceholderAsync(
        ExternalApiSource source,
        DateOnly businessDate,
        JobTriggerType triggeredBy,
        int? triggeredByUserId,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var jobRun = new JobRun
        {
            JobName = GetJobName(source),
            BusinessDate = businessDate,
            TriggeredBy = triggeredBy,
            TriggeredByUserId = triggeredByUserId,
            StartedAtUtc = nowUtc,
            Status = JobRunStatus.Running,
        };

        dbContext.JobRuns.Add(jobRun);
        await dbContext.SaveChangesAsync(cancellationToken);

        var parks = await dbContext.Parks
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Status == RecordStatus.Active)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        jobRun.TotalItems = parks.Count;

        foreach (var park in parks)
        {
            var itemStartedAtUtc = DateTime.UtcNow;
            var item = new JobRunItem
            {
                JobRunId = jobRun.Id,
                BusinessDate = businessDate,
                ParkId = park.Id,
                Source = source,
                Status = JobRunItemStatus.Failed,
                AttemptCount = 0,
                StartedAtUtc = itemStartedAtUtc,
                FinishedAtUtc = itemStartedAtUtc,
                DurationMs = 0,
                ErrorCode = "ExternalApiNotConfigured",
                ErrorMessage = "API bên ngoài chưa được cấu hình. Vui lòng nhập tay dữ liệu nếu cần đối soát ngày này.",
            };

            dbContext.JobRunItems.Add(item);
            await dbContext.SaveChangesAsync(cancellationToken);

            var rawResponse = new ExternalApiRawResponse
            {
                Source = source,
                BusinessDate = businessDate,
                ParkId = park.Id,
                JobRunId = jobRun.Id,
                JobRunItemId = item.Id,
                RequestPayloadJson = JsonSerializer.Serialize(new
                {
                    park.Id,
                    park.Code,
                    businessDate,
                    source = source.ToString(),
                    message = "External API is not configured.",
                }),
                ResponseBodyJson = JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "External API is not configured.",
                }),
                IsSuccess = false,
                ErrorMessage = item.ErrorMessage,
                DurationMs = 0,
                ReceivedAtUtc = itemStartedAtUtc,
            };

            dbContext.ExternalApiRawResponses.Add(rawResponse);
            await dbContext.SaveChangesAsync(cancellationToken);

            item.RawResponseId = rawResponse.Id;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        jobRun.FinishedAtUtc = DateTime.UtcNow;
        jobRun.FailedItems = parks.Count;
        jobRun.SuccessItems = 0;
        jobRun.SkippedItems = 0;
        jobRun.Status = parks.Count == 0 ? JobRunStatus.Succeeded : JobRunStatus.CompletedWithErrors;
        jobRun.ErrorMessage = parks.Count == 0
            ? null
            : "API bên ngoài chưa được cấu hình. Các KVC cần được xử lý thủ công nếu phát sinh dữ liệu.";
        jobRun.SummaryJson = JsonSerializer.Serialize(new
        {
            source = source.ToString(),
            businessDate,
            activeParkCount = parks.Count,
            externalApiConfigured = false,
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await LogJobRunAsync(jobRun, triggeredByUserId, cancellationToken);

        return await BuildJobRunDetailAsync(jobRun.Id, cancellationToken);
    }

    private Task LogJobRunAsync(JobRun jobRun, int? triggeredByUserId, CancellationToken cancellationToken)
    {
        return auditService.LogAsync(new AuditLogEntry
        {
            UserId = triggeredByUserId,
            Module = "Jobs",
            EntityName = "JobRun",
            EntityId = jobRun.Id.ToString(),
            Action = AuditAction.RunJob,
            After = new
            {
                jobRun.Id,
                jobRun.JobName,
                jobRun.BusinessDate,
                jobRun.Status,
                jobRun.TotalItems,
                jobRun.SuccessItems,
                jobRun.FailedItems,
                jobRun.SkippedItems,
            },
        }, cancellationToken);
    }

    private async Task<JobRunDetailDto> BuildJobRunDetailAsync(int jobRunId, CancellationToken cancellationToken)
    {
        var jobRun = await dbContext.JobRuns.AsNoTracking().FirstAsync(x => x.Id == jobRunId, cancellationToken);
        var items = await (
            from item in dbContext.JobRunItems.AsNoTracking()
            join park in dbContext.Parks.AsNoTracking() on item.ParkId equals park.Id into parkGroup
            from park in parkGroup.DefaultIfEmpty()
            where item.JobRunId == jobRunId
            orderby item.Id
            select new JobRunItemDto
            {
                Id = item.Id,
                JobRunId = item.JobRunId,
                BusinessDate = item.BusinessDate,
                ParkId = item.ParkId,
                ParkCode = park != null ? park.Code : null,
                ParkName = park != null ? park.Name : null,
                Source = item.Source,
                Status = item.Status,
                AttemptCount = item.AttemptCount,
                StartedAtUtc = item.StartedAtUtc,
                FinishedAtUtc = item.FinishedAtUtc,
                DurationMs = item.DurationMs,
                ErrorCode = item.ErrorCode,
                ErrorMessage = item.ErrorMessage,
                RawResponseId = item.RawResponseId,
                ResolvedByUserId = item.ResolvedByUserId,
                ResolvedAtUtc = item.ResolvedAtUtc,
                ManualResolutionNote = item.ManualResolutionNote,
            })
            .ToListAsync(cancellationToken);

        return new JobRunDetailDto
        {
            Id = jobRun.Id,
            JobName = jobRun.JobName,
            BusinessDate = jobRun.BusinessDate,
            TriggeredBy = jobRun.TriggeredBy,
            TriggeredByUserId = jobRun.TriggeredByUserId,
            StartedAtUtc = jobRun.StartedAtUtc,
            FinishedAtUtc = jobRun.FinishedAtUtc,
            Status = jobRun.Status,
            TotalItems = jobRun.TotalItems,
            SuccessItems = jobRun.SuccessItems,
            FailedItems = jobRun.FailedItems,
            SkippedItems = jobRun.SkippedItems,
            ErrorMessage = jobRun.ErrorMessage,
            SummaryJson = jobRun.SummaryJson,
            Items = items,
        };
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
