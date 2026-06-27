using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Jobs;
using PpvRecon.Domain.Entities.Jobs;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Services;

public interface IMaintenanceJobService
{
    Task<SendSyncErrorSummaryResultDto> SendSyncErrorSummaryAsync(
        DateOnly businessDate,
        int? triggeredByUserId,
        CancellationToken cancellationToken = default,
        JobTriggerType triggeredBy = JobTriggerType.Manual);

    Task<CleanupAuditLogsResultDto> CleanupAuditLogsAsync(
        int? triggeredByUserId,
        CancellationToken cancellationToken = default,
        JobTriggerType triggeredBy = JobTriggerType.Manual);
}

public sealed class MaintenanceJobService(
    PpvReconDbContext dbContext,
    IEmailSender emailSender,
    IAuditService auditService) : IMaintenanceJobService
{
    public async Task<SendSyncErrorSummaryResultDto> SendSyncErrorSummaryAsync(
        DateOnly businessDate,
        int? triggeredByUserId,
        CancellationToken cancellationToken = default,
        JobTriggerType triggeredBy = JobTriggerType.Manual)
    {
        var nowUtc = DateTime.UtcNow;
        var jobRun = new JobRun
        {
            JobName = "SendDailySyncErrorSummary",
            BusinessDate = businessDate,
            TriggeredBy = triggeredBy,
            TriggeredByUserId = triggeredByUserId,
            StartedAtUtc = nowUtc,
            Status = JobRunStatus.Running,
        };

        dbContext.JobRuns.Add(jobRun);
        await dbContext.SaveChangesAsync(cancellationToken);

        var errors = await (
            from item in dbContext.JobRunItems.AsNoTracking()
            join run in dbContext.JobRuns.AsNoTracking() on item.JobRunId equals run.Id
            join park in dbContext.Parks.AsNoTracking() on item.ParkId equals park.Id into parkGroup
            from park in parkGroup.DefaultIfEmpty()
            where item.BusinessDate == businessDate && item.Status == JobRunItemStatus.Failed
            orderby run.JobName, park != null ? park.Name : string.Empty
            select new
            {
                item.Id,
                run.JobName,
                item.Source,
                item.ParkId,
                ParkCode = park != null ? park.Code : null,
                ParkName = park != null ? park.Name : null,
                item.ErrorCode,
                item.ErrorMessage,
                item.AttemptCount,
                item.FinishedAtUtc,
            })
            .ToListAsync(cancellationToken);

        var recipients = await dbContext.NotificationRecipients
            .AsNoTracking()
            .Where(x => x.NotificationType == NotificationType.SyncErrorSummary && x.IsActive)
            .Select(x => x.Email)
            .ToListAsync(cancellationToken);

        var result = new SendSyncErrorSummaryResultDto
        {
            JobRunId = jobRun.Id,
            BusinessDate = businessDate,
            ErrorCount = errors.Count,
            RecipientCount = recipients.Count,
            EmailAttempted = false,
            EmailSent = false,
        };

        if (errors.Count == 0)
        {
            result.Message = "Không có lỗi đồng bộ cần gửi email.";
        }
        else
        {
            var emailResult = await emailSender.SendAsync(
                recipients,
                $"[PpvRecon] Tổng hợp lỗi đồng bộ ngày {businessDate:yyyy-MM-dd}",
                BuildErrorSummaryBody(businessDate, errors),
                cancellationToken);

            result.EmailAttempted = emailResult.Attempted;
            result.EmailSent = emailResult.Sent;
            result.Message = emailResult.Message;
        }

        jobRun.TotalItems = errors.Count;
        jobRun.SuccessItems = errors.Count;
        jobRun.FailedItems = 0;
        jobRun.Status = JobRunStatus.Succeeded;
        jobRun.FinishedAtUtc = DateTime.UtcNow;
        jobRun.SummaryJson = JsonSerializer.Serialize(result);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            UserId = triggeredByUserId,
            Module = "Jobs",
            EntityName = "JobRun",
            EntityId = jobRun.Id.ToString(),
            Action = AuditAction.RunJob,
            After = result,
        }, cancellationToken);

        return result;
    }

    public async Task<CleanupAuditLogsResultDto> CleanupAuditLogsAsync(
        int? triggeredByUserId,
        CancellationToken cancellationToken = default,
        JobTriggerType triggeredBy = JobTriggerType.Manual)
    {
        var retentionDaysText = await dbContext.SystemSettings
            .AsNoTracking()
            .Where(x => x.Key == "Audit.RetentionDays")
            .Select(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);

        var retentionDays = int.TryParse(retentionDaysText, out var parsedRetentionDays)
            ? parsedRetentionDays
            : 365;

        var cutoffUtc = DateTime.UtcNow.AddDays(-retentionDays);
        var jobRun = new JobRun
        {
            JobName = "CleanupAuditLogs",
            TriggeredBy = triggeredBy,
            TriggeredByUserId = triggeredByUserId,
            StartedAtUtc = DateTime.UtcNow,
            Status = JobRunStatus.Running,
        };

        dbContext.JobRuns.Add(jobRun);
        await dbContext.SaveChangesAsync(cancellationToken);

        var deletedCount = await dbContext.AuditLogs
            .Where(x => x.OccurredAtUtc < cutoffUtc)
            .ExecuteDeleteAsync(cancellationToken);

        var result = new CleanupAuditLogsResultDto
        {
            JobRunId = jobRun.Id,
            RetentionDays = retentionDays,
            CutoffUtc = cutoffUtc,
            DeletedCount = deletedCount,
        };

        jobRun.TotalItems = deletedCount;
        jobRun.SuccessItems = deletedCount;
        jobRun.Status = JobRunStatus.Succeeded;
        jobRun.FinishedAtUtc = DateTime.UtcNow;
        jobRun.SummaryJson = JsonSerializer.Serialize(result);

        await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    private static string BuildErrorSummaryBody(DateOnly businessDate, IEnumerable<dynamic> errors)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Ngày dữ liệu: {businessDate:yyyy-MM-dd}");
        builder.AppendLine();
        builder.AppendLine("Có lỗi đồng bộ cần kế toán kiểm tra và nhập tay nếu cần.");
        builder.AppendLine();

        foreach (var error in errors)
        {
            builder.AppendLine($"- Job: {error.JobName}");
            builder.AppendLine($"  Source: {error.Source}");
            builder.AppendLine($"  KVC: {error.ParkCode} - {error.ParkName}");
            builder.AppendLine($"  Error: {error.ErrorCode} - {error.ErrorMessage}");
            builder.AppendLine($"  Attempts: {error.AttemptCount}");
            builder.AppendLine();
        }

        builder.AppendLine("Vui lòng vào màn Khu vui chơi > Lỗi đồng bộ cần xử lý để kiểm tra.");
        return builder.ToString();
    }
}
