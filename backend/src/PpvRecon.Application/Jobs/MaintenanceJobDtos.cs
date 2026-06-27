namespace PpvRecon.Application.Jobs;

public sealed class SendSyncErrorSummaryResultDto
{
    public int JobRunId { get; set; }
    public DateOnly BusinessDate { get; set; }
    public int ErrorCount { get; set; }
    public int RecipientCount { get; set; }
    public bool EmailAttempted { get; set; }
    public bool EmailSent { get; set; }
    public string? Message { get; set; }
}

public sealed class CleanupAuditLogsResultDto
{
    public int JobRunId { get; set; }
    public int RetentionDays { get; set; }
    public DateTime CutoffUtc { get; set; }
    public int DeletedCount { get; set; }
}
