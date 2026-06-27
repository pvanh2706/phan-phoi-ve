using PpvRecon.Domain.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Summaries;

public sealed class DailyBankTransactionSummary : IAuditableEntity
{
    public int Id { get; set; }
    public DateOnly BusinessDate { get; set; }
    public int ParkId { get; set; }
    public ParkPaymentType PaymentType { get; set; }
    public BankTransactionType TransactionType { get; set; }
    public long TotalDebitAmount { get; set; }
    public long TotalCreditAmount { get; set; }
    public int TransactionCount { get; set; }
    public SourceType SourceType { get; set; }
    public int? SourceJobRunId { get; set; }
    public int? SourceJobRunItemId { get; set; }
    public int? RawResponseId { get; set; }
    public string? ManualReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
}
