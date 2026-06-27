using System.ComponentModel.DataAnnotations;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Application.Reconciliation;

public sealed class BuildReconciliationRequest
{
    public DateOnly BusinessDate { get; set; }
}

public sealed class BuildReconciliationResultDto
{
    public int JobRunId { get; set; }
    public DateOnly BusinessDate { get; set; }
    public int TotalItems { get; set; }
    public int MatchedCount { get; set; }
    public int VarianceCount { get; set; }
    public int MissingDataCount { get; set; }
    public int ResolvedPreservedCount { get; set; }
}

public sealed class ParkReconciliationDto
{
    public int Id { get; set; }
    public DateOnly BusinessDate { get; set; }
    public DateOnly? PreviousBusinessDate { get; set; }
    public int ParkId { get; set; }
    public string ParkCode { get; set; } = string.Empty;
    public string ParkName { get; set; } = string.Empty;
    public ParkPaymentType PaymentType { get; set; }
    public long? PreviousBalance { get; set; }
    public long? AdditionalAmount { get; set; }
    public long? UsedAmount { get; set; }
    public long? ExpectedBalance { get; set; }
    public long? ActualBalance { get; set; }
    public long? VarianceAmount { get; set; }
    public long? AdjustmentAmount { get; set; }
    public string? AdjustmentNote { get; set; }
    public ReconciliationStatus Status { get; set; }
    public bool MissingPreviousBalance { get; set; }
    public bool MissingActualBalance { get; set; }
    public bool MissingTicketCost { get; set; }
    public bool MissingBankTransaction { get; set; }
    public int? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public int? LastBuiltJobRunId { get; set; }
    public int RebuildCount { get; set; }
    public string? LastSourceHash { get; set; }
    public string? ResolvedSourceHash { get; set; }
    public bool SourceChangedAfterResolved { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class ResolveReconciliationRequest
{
    public long AdjustmentAmount { get; set; }

    [Required(ErrorMessage = "Ghi chú xử lý là bắt buộc.")]
    [MaxLength(2000, ErrorMessage = "Ghi chú xử lý không được vượt quá 2000 ký tự.")]
    public string AdjustmentNote { get; set; } = string.Empty;
}

public interface IReconciliationBuilder
{
    Task<BuildReconciliationResultDto> BuildAsync(
        DateOnly businessDate,
        int? triggeredByUserId,
        CancellationToken cancellationToken = default,
        JobTriggerType triggeredBy = JobTriggerType.Manual);
}
