using PpvRecon.Domain.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Reconciliation;

public sealed class ParkReconciliation : IAuditableEntity
{
    public int Id { get; set; }
    public DateOnly BusinessDate { get; set; }
    public DateOnly? PreviousBusinessDate { get; set; }
    public int ParkId { get; set; }
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
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
}
