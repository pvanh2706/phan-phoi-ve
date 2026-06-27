using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Reconciliation;
using PpvRecon.Domain.Entities.Jobs;
using PpvRecon.Domain.Entities.Parks;
using PpvRecon.Domain.Entities.Reconciliation;
using PpvRecon.Domain.Entities.Summaries;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Services;

public sealed class ReconciliationBuilder(
    PpvReconDbContext dbContext,
    IAuditService auditService) : IReconciliationBuilder
{
    public async Task<BuildReconciliationResultDto> BuildAsync(
        DateOnly businessDate,
        int? triggeredByUserId,
        CancellationToken cancellationToken = default,
        JobTriggerType triggeredBy = JobTriggerType.Manual)
    {
        var nowUtc = DateTime.UtcNow;
        var previousBusinessDate = businessDate.AddDays(-1);
        var parks = await dbContext.Parks
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Status == RecordStatus.Active)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var jobRun = new JobRun
        {
            JobName = "BuildParkReconciliation",
            BusinessDate = businessDate,
            TriggeredBy = triggeredBy,
            TriggeredByUserId = triggeredByUserId,
            StartedAtUtc = nowUtc,
            Status = JobRunStatus.Running,
            TotalItems = parks.Count,
        };

        dbContext.JobRuns.Add(jobRun);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = new BuildReconciliationResultDto
        {
            JobRunId = jobRun.Id,
            BusinessDate = businessDate,
            TotalItems = parks.Count,
        };

        foreach (var park in parks)
        {
            var previousBalance = await dbContext.DailyParkBalanceSnapshots
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BusinessDate == previousBusinessDate && x.ParkId == park.Id, cancellationToken);
            var actualBalance = await dbContext.DailyParkBalanceSnapshots
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BusinessDate == businessDate && x.ParkId == park.Id, cancellationToken);
            var ticketCost = await dbContext.DailyTicketCostSummaries
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BusinessDate == businessDate && x.ParkId == park.Id, cancellationToken);
            var bankSummaries = await dbContext.DailyBankTransactionSummaries
                .AsNoTracking()
                .Where(x => x.BusinessDate == businessDate && x.ParkId == park.Id)
                .ToListAsync(cancellationToken);

            var missingPreviousBalance = previousBalance is null;
            var missingActualBalance = actualBalance is null;
            var missingTicketCost = ticketCost is null;
            var missingBankTransaction = bankSummaries.Count == 0;

            long? previousBalanceValue = previousBalance?.AvailableBalance;
            long? actualBalanceValue = actualBalance?.AvailableBalance;
            long? usedAmount = ticketCost?.TotalTicketCost;
            long? additionalAmount = bankSummaries.Count == 0
                ? null
                : bankSummaries.Sum(x => x.TotalCreditAmount - x.TotalDebitAmount);

            long? expectedBalance = null;
            long? varianceAmount = null;
            ReconciliationStatus calculatedStatus;

            if (missingPreviousBalance || missingActualBalance || missingTicketCost || missingBankTransaction)
            {
                calculatedStatus = ReconciliationStatus.MissingData;
            }
            else
            {
                expectedBalance = previousBalanceValue + additionalAmount - usedAmount;
                varianceAmount = actualBalanceValue - expectedBalance;
                calculatedStatus = varianceAmount == 0
                    ? ReconciliationStatus.Matched
                    : ReconciliationStatus.Variance;
            }

            var sourceHash = BuildSourceHash(
                park,
                businessDate,
                previousBusinessDate,
                previousBalance,
                actualBalance,
                ticketCost,
                bankSummaries,
                additionalAmount,
                usedAmount);

            var reconciliation = await dbContext.ParkReconciliations
                .FirstOrDefaultAsync(x => x.BusinessDate == businessDate && x.ParkId == park.Id, cancellationToken);

            if (reconciliation is null)
            {
                reconciliation = new ParkReconciliation
                {
                    BusinessDate = businessDate,
                    ParkId = park.Id,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedByUserId = triggeredByUserId,
                };
                dbContext.ParkReconciliations.Add(reconciliation);
            }
            else
            {
                reconciliation.UpdatedAtUtc = DateTime.UtcNow;
                reconciliation.UpdatedByUserId = triggeredByUserId;
            }

            var wasResolved = reconciliation.Status == ReconciliationStatus.Resolved;

            reconciliation.PreviousBusinessDate = previousBusinessDate;
            reconciliation.PaymentType = park.PaymentType;
            reconciliation.PreviousBalance = previousBalanceValue;
            reconciliation.AdditionalAmount = additionalAmount;
            reconciliation.UsedAmount = usedAmount;
            reconciliation.ExpectedBalance = expectedBalance;
            reconciliation.ActualBalance = actualBalanceValue;
            reconciliation.VarianceAmount = varianceAmount;
            reconciliation.MissingPreviousBalance = missingPreviousBalance;
            reconciliation.MissingActualBalance = missingActualBalance;
            reconciliation.MissingTicketCost = missingTicketCost;
            reconciliation.MissingBankTransaction = missingBankTransaction;
            reconciliation.LastBuiltJobRunId = jobRun.Id;
            reconciliation.RebuildCount += 1;
            reconciliation.LastSourceHash = sourceHash;

            if (wasResolved)
            {
                reconciliation.SourceChangedAfterResolved = reconciliation.ResolvedSourceHash != sourceHash;
                result.ResolvedPreservedCount += 1;
            }
            else
            {
                reconciliation.Status = calculatedStatus;
                reconciliation.SourceChangedAfterResolved = false;
            }

            switch (reconciliation.Status)
            {
                case ReconciliationStatus.Matched:
                    result.MatchedCount += 1;
                    break;
                case ReconciliationStatus.Variance:
                    result.VarianceCount += 1;
                    break;
                case ReconciliationStatus.MissingData:
                    result.MissingDataCount += 1;
                    break;
            }

            dbContext.JobRunItems.Add(new JobRunItem
            {
                JobRunId = jobRun.Id,
                BusinessDate = businessDate,
                ParkId = park.Id,
                Source = null,
                Status = JobRunItemStatus.Succeeded,
                AttemptCount = 1,
                StartedAtUtc = nowUtc,
                FinishedAtUtc = DateTime.UtcNow,
                DurationMs = 0,
            });
        }

        jobRun.Status = JobRunStatus.Succeeded;
        jobRun.SuccessItems = parks.Count;
        jobRun.FailedItems = 0;
        jobRun.FinishedAtUtc = DateTime.UtcNow;
        jobRun.SummaryJson = JsonSerializer.Serialize(result);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            UserId = triggeredByUserId,
            Module = "Park",
            EntityName = "ParkReconciliation",
            EntityId = businessDate.ToString("yyyy-MM-dd"),
            Action = AuditAction.RunJob,
            After = result,
        }, cancellationToken);

        return result;
    }

    private static string BuildSourceHash(
        Park park,
        DateOnly businessDate,
        DateOnly previousBusinessDate,
        DailyParkBalanceSnapshot? previousBalance,
        DailyParkBalanceSnapshot? actualBalance,
        DailyTicketCostSummary? ticketCost,
        IReadOnlyList<DailyBankTransactionSummary> bankSummaries,
        long? additionalAmount,
        long? usedAmount)
    {
        var source = new
        {
            park.Id,
            businessDate,
            previousBusinessDate,
            previousBalance = previousBalance is null ? null : new
            {
                previousBalance.Id,
                previousBalance.AvailableBalance,
                previousBalance.SourceType,
            },
            actualBalance = actualBalance is null ? null : new
            {
                actualBalance.Id,
                actualBalance.AvailableBalance,
                actualBalance.SourceType,
            },
            ticketCost = ticketCost is null ? null : new
            {
                ticketCost.Id,
                ticketCost.TotalTicketCost,
                ticketCost.SourceType,
            },
            bankSummaries = bankSummaries
                .OrderBy(x => x.TransactionType)
                .Select(x => new
                {
                    x.Id,
                    x.TransactionType,
                    x.TotalDebitAmount,
                    x.TotalCreditAmount,
                    x.TransactionCount,
                    x.SourceType,
                }),
            additionalAmount,
            usedAmount,
        };

        var json = JsonSerializer.Serialize(source);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash);
    }
}
