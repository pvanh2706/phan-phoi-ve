using PpvRecon.Domain.Enums;

namespace PpvRecon.Application.Jobs;

public sealed class RunJobRequest
{
    public DateOnly? BusinessDate { get; set; }
}

public class JobRunListItemDto
{
    public int Id { get; set; }
    public string JobName { get; set; } = string.Empty;
    public DateOnly? BusinessDate { get; set; }
    public JobTriggerType TriggeredBy { get; set; }
    public int? TriggeredByUserId { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public JobRunStatus Status { get; set; }
    public int TotalItems { get; set; }
    public int SuccessItems { get; set; }
    public int FailedItems { get; set; }
    public int SkippedItems { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class JobRunDetailDto : JobRunListItemDto
{
    public string? SummaryJson { get; set; }
    public IReadOnlyList<JobRunItemDto> Items { get; set; } = [];
}

public sealed class JobRunItemDto
{
    public int Id { get; set; }
    public int JobRunId { get; set; }
    public DateOnly? BusinessDate { get; set; }
    public int? ParkId { get; set; }
    public string? ParkCode { get; set; }
    public string? ParkName { get; set; }
    public ExternalApiSource? Source { get; set; }
    public JobRunItemStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public int? DurationMs { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public int? RawResponseId { get; set; }
    public int? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public string? ManualResolutionNote { get; set; }
}

public interface IJobRunner
{
    Task<JobRunDetailDto> RunExternalSyncPlaceholderAsync(
        ExternalApiSource source,
        DateOnly businessDate,
        JobTriggerType triggeredBy,
        int? triggeredByUserId,
        CancellationToken cancellationToken = default);
}
