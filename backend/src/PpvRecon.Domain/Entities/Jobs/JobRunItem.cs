using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Jobs;

public sealed class JobRunItem
{
    public int Id { get; set; }
    public int JobRunId { get; set; }
    public DateOnly? BusinessDate { get; set; }
    public int? ParkId { get; set; }
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
