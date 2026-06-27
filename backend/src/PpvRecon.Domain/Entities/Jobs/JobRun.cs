using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Jobs;

public sealed class JobRun
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
    public string? SummaryJson { get; set; }
}
