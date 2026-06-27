using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Jobs;
using PpvRecon.Domain.Entities.Jobs;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Services;

public sealed class JobRunner(
    PpvReconDbContext dbContext,
    IAuditService auditService) : IJobRunner
{
    public async Task<JobRunDetailDto> RunExternalSyncPlaceholderAsync(
        ExternalApiSource source,
        DateOnly businessDate,
        JobTriggerType triggeredBy,
        int? triggeredByUserId,
        CancellationToken cancellationToken = default)
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
                RequestUrl = null,
                RequestPayloadJson = JsonSerializer.Serialize(new
                {
                    park.Id,
                    park.Code,
                    businessDate,
                    source = source.ToString(),
                    message = "External API is not configured.",
                }),
                ResponseStatusCode = null,
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
        await auditService.LogAsync(new AuditLogEntry
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
                jobRun.FailedItems,
            },
        }, cancellationToken);

        return await BuildJobRunDetailAsync(jobRun.Id, cancellationToken);
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
