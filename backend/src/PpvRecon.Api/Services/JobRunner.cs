using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Api.Services.BankStatement;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Jobs;
using PpvRecon.Application.Reconciliation;
using PpvRecon.Domain.Entities.Jobs;
using PpvRecon.Domain.Entities.Parks;
using PpvRecon.Domain.Entities.Summaries;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Services;

public sealed class JobRunner(
    PpvReconDbContext dbContext,
    IParkBalanceApiClient parkBalanceApiClient,
    ITicketCostSyncService ticketCostSyncService,
    IBankStatementSyncService bankStatementSyncService,
    IReconciliationBuilder reconciliationBuilder,
    IAuditService auditService,
    ILogger<JobRunner> logger) : IJobRunner
{
    /// <summary>Kết quả một lần sync: payload tóm tắt + các ngày nghiệp vụ có dữ liệu mới (để tự build lại đối soát).</summary>
    private sealed record ExternalSyncOutcome(object Summary, IReadOnlyList<DateOnly> AffectedBusinessDates);

    public Task<JobRunDetailDto> RunExternalSyncAsync(
        ExternalApiSource source,
        DateOnly businessDate,
        JobTriggerType triggeredBy,
        int? triggeredByUserId,
        CancellationToken cancellationToken = default)
    {
        return source switch
        {
            ExternalApiSource.ParkBalance =>
                RunParkBalanceSyncAsync(source, businessDate, triggeredBy, triggeredByUserId, cancellationToken),
            ExternalApiSource.TicketCost =>
                RunServiceBackedSyncAsync(
                    source,
                    businessDate,
                    triggeredBy,
                    triggeredByUserId,
                    async ct =>
                    {
                        var r = await ticketCostSyncService.SyncTodayAsync(triggeredByUserId, ct);
                        return new ExternalSyncOutcome(
                            new
                            {
                                r.BusinessDate,
                                r.TotalLines,
                                r.Imported,
                                r.SkippedUnmatched,
                                r.UnmatchedParkCodes,
                            },
                            [r.BusinessDate]);
                    },
                    cancellationToken),
            ExternalApiSource.BankTransaction =>
                RunServiceBackedSyncAsync(
                    source,
                    businessDate,
                    triggeredBy,
                    triggeredByUserId,
                    async ct =>
                    {
                        var r = await bankStatementSyncService.SyncAsync(businessDate, triggeredByUserId, ct);
                        return new ExternalSyncOutcome(r, r.OverwrittenBusinessDates);
                    },
                    cancellationToken),
            _ => RunExternalSyncPlaceholderAsync(source, businessDate, triggeredBy, triggeredByUserId, cancellationToken),
        };
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
                // Áp "Quy tắc số dư" của KVC (vd MultiplyMinusOne = đảo dấu) trước khi lưu snapshot.
                var transformedBalance = ApplyBalanceTransform(apiResult.AvailableBalance.Value, park.BalanceTransformRule);
                await UpsertParkBalanceSnapshotAsync(
                    park,
                    businessDate,
                    transformedBalance,
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

        if (jobRun.SuccessItems > 0)
        {
            await AutoBuildReconciliationAsync([businessDate], triggeredByUserId, cancellationToken);
        }

        return await BuildJobRunDetailAsync(jobRun.Id, cancellationToken);
    }

    /// <summary>
    /// Áp quy tắc biến đổi số dư của KVC (Park.BalanceTransformRule):
    /// "MultiplyMinusOne" = nhân -1 (đảo dấu); "None"/null/khác = giữ nguyên.
    /// </summary>
    private static long ApplyBalanceTransform(long rawBalance, string? rule)
        => string.Equals(rule?.Trim(), "MultiplyMinusOne", StringComparison.OrdinalIgnoreCase)
            ? -rawBalance
            : rawBalance;

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

    /// <summary>
    /// Chạy một job đồng bộ dựa trên service tổng hợp (TicketCost/BankTransaction): tạo JobRun,
    /// gọi service thật, ghi 1 JobRunItem tóm tắt + 1 bản ghi log gọi API, và cập nhật trạng thái.
    /// </summary>
    private async Task<JobRunDetailDto> RunServiceBackedSyncAsync(
        ExternalApiSource source,
        DateOnly businessDate,
        JobTriggerType triggeredBy,
        int? triggeredByUserId,
        Func<CancellationToken, Task<ExternalSyncOutcome>> syncAction,
        CancellationToken cancellationToken)
    {
        var startedAtUtc = DateTime.UtcNow;
        var jobRun = new JobRun
        {
            JobName = GetJobName(source),
            BusinessDate = businessDate,
            TriggeredBy = triggeredBy,
            TriggeredByUserId = triggeredByUserId,
            StartedAtUtc = startedAtUtc,
            Status = JobRunStatus.Running,
            TotalItems = 1,
        };

        dbContext.JobRuns.Add(jobRun);
        await dbContext.SaveChangesAsync(cancellationToken);

        var item = new JobRunItem
        {
            JobRunId = jobRun.Id,
            BusinessDate = businessDate,
            ParkId = null,
            Source = source,
            Status = JobRunItemStatus.Running,
            AttemptCount = 1,
            StartedAtUtc = startedAtUtc,
        };

        dbContext.JobRunItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        object? summaryPayload = null;
        IReadOnlyList<DateOnly> affectedBusinessDates = [];
        string? errorMessage = null;
        bool isSuccess;
        try
        {
            var outcome = await syncAction(cancellationToken);
            summaryPayload = outcome.Summary;
            affectedBusinessDates = outcome.AffectedBusinessDates;
            isSuccess = true;
        }
        catch (Exception ex)
        {
            isSuccess = false;
            errorMessage = ex.Message;
        }

        var finishedAtUtc = DateTime.UtcNow;
        var durationMs = (int)Math.Clamp((finishedAtUtc - startedAtUtc).TotalMilliseconds, 0, int.MaxValue);

        var responseBody = isSuccess
            ? (object)new { success = true, result = summaryPayload }
            : new { success = false, message = errorMessage };

        var rawResponse = new ExternalApiRawResponse
        {
            Source = source,
            BusinessDate = businessDate,
            ParkId = null,
            JobRunId = jobRun.Id,
            JobRunItemId = item.Id,
            RequestPayloadJson = JsonSerializer.Serialize(new
            {
                source = source.ToString(),
                businessDate,
                triggeredBy = triggeredBy.ToString(),
            }),
            ResponseBodyJson = JsonSerializer.Serialize(responseBody),
            IsSuccess = isSuccess,
            ErrorMessage = isSuccess ? null : errorMessage,
            DurationMs = durationMs,
            ReceivedAtUtc = finishedAtUtc,
        };

        dbContext.ExternalApiRawResponses.Add(rawResponse);
        await dbContext.SaveChangesAsync(cancellationToken);

        item.Status = isSuccess ? JobRunItemStatus.Succeeded : JobRunItemStatus.Failed;
        item.FinishedAtUtc = finishedAtUtc;
        item.DurationMs = durationMs;
        item.RawResponseId = rawResponse.Id;
        if (!isSuccess)
        {
            item.ErrorCode = "ExternalSyncFailed";
            item.ErrorMessage = errorMessage;
        }

        jobRun.FinishedAtUtc = finishedAtUtc;
        jobRun.SuccessItems = isSuccess ? 1 : 0;
        jobRun.FailedItems = isSuccess ? 0 : 1;
        jobRun.SkippedItems = 0;
        jobRun.Status = isSuccess ? JobRunStatus.Succeeded : JobRunStatus.CompletedWithErrors;
        jobRun.ErrorMessage = isSuccess ? null : errorMessage;
        jobRun.SummaryJson = JsonSerializer.Serialize(new
        {
            source = source.ToString(),
            businessDate,
            success = isSuccess,
            result = summaryPayload,
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await LogJobRunAsync(jobRun, triggeredByUserId, cancellationToken);
        await AutoBuildReconciliationAsync(affectedBusinessDates, triggeredByUserId, cancellationToken);

        return await BuildJobRunDetailAsync(jobRun.Id, cancellationToken);
    }

    /// <summary>
    /// Tự động build lại đối soát cho các ngày vừa nhận được dữ liệu từ API ngoài,
    /// để màn "Đối soát Khu vui chơi" luôn phản ánh số liệu mới nhất mà không cần bấm "Build đối soát".
    /// Build lỗi không làm hỏng kết quả job đồng bộ (chỉ ghi log).
    /// </summary>
    private async Task AutoBuildReconciliationAsync(
        IReadOnlyList<DateOnly> businessDates,
        int? triggeredByUserId,
        CancellationToken cancellationToken)
    {
        foreach (var date in businessDates.Distinct().OrderBy(x => x))
        {
            try
            {
                await reconciliationBuilder.BuildAsync(date, triggeredByUserId, cancellationToken, JobTriggerType.System);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Tự động build đối soát cho ngày {BusinessDate} thất bại.", date);
            }
        }
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
