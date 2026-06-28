using PpvRecon.Domain.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Summaries;

public sealed class DailyParkBalanceSnapshot : IAuditableEntity
{
    public int Id { get; set; }
    public DateOnly BusinessDate { get; set; }
    public int ParkId { get; set; }
    public ParkPaymentType PaymentType { get; set; }
    public long AvailableBalance { get; set; }
    public string? BankAccountSnapshot { get; set; }
    public SourceType SourceType { get; set; }
    public int? SourceJobRunId { get; set; }
    public int? SourceJobRunItemId { get; set; }
    public int? RawResponseId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
}
