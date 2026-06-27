using System.ComponentModel.DataAnnotations;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Application.Summaries;

public sealed class DailyParkBalanceSnapshotDto
{
    public int Id { get; set; }
    public DateOnly BusinessDate { get; set; }
    public int ParkId { get; set; }
    public string ParkCode { get; set; } = string.Empty;
    public string ParkName { get; set; } = string.Empty;
    public ParkPaymentType PaymentType { get; set; }
    public long AvailableBalance { get; set; }
    public long? CurrentDebt { get; set; }
    public string? BankAccountSnapshot { get; set; }
    public SourceType SourceType { get; set; }
    public int? SourceJobRunId { get; set; }
    public int? SourceJobRunItemId { get; set; }
    public int? RawResponseId { get; set; }
    public string? ManualReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class DailyTicketCostSummaryDto
{
    public int Id { get; set; }
    public DateOnly BusinessDate { get; set; }
    public int ParkId { get; set; }
    public string ParkCode { get; set; } = string.Empty;
    public string ParkName { get; set; } = string.Empty;
    public ParkPaymentType PaymentType { get; set; }
    public long TotalTicketCost { get; set; }
    public long? TotalSalesAmount { get; set; }
    public int? TotalQuantity { get; set; }
    public SourceType SourceType { get; set; }
    public int? SourceJobRunId { get; set; }
    public int? SourceJobRunItemId { get; set; }
    public int? RawResponseId { get; set; }
    public string? ManualReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class DailyBankTransactionSummaryDto
{
    public int Id { get; set; }
    public DateOnly BusinessDate { get; set; }
    public int ParkId { get; set; }
    public string ParkCode { get; set; } = string.Empty;
    public string ParkName { get; set; } = string.Empty;
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
    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class ManualParkBalanceRequest
{
    public DateOnly BusinessDate { get; set; }
    public int ParkId { get; set; }
    public long AvailableBalance { get; set; }
    public long? CurrentDebt { get; set; }
    [MaxLength(100)]
    public string? BankAccountSnapshot { get; set; }
    [Required(ErrorMessage = "Lý do nhập tay là bắt buộc.")]
    [MaxLength(1000, ErrorMessage = "Lý do nhập tay không được vượt quá 1000 ký tự.")]
    public string ManualReason { get; set; } = string.Empty;
    public int? JobRunItemId { get; set; }
}

public sealed class ManualTicketCostSummaryRequest
{
    public DateOnly BusinessDate { get; set; }
    public int ParkId { get; set; }
    public long TotalTicketCost { get; set; }
    public long? TotalSalesAmount { get; set; }
    public int? TotalQuantity { get; set; }
    [Required(ErrorMessage = "Lý do nhập tay là bắt buộc.")]
    [MaxLength(1000, ErrorMessage = "Lý do nhập tay không được vượt quá 1000 ký tự.")]
    public string ManualReason { get; set; } = string.Empty;
    public int? JobRunItemId { get; set; }
}

public sealed class ManualBankTransactionSummaryRequest
{
    public DateOnly BusinessDate { get; set; }
    public int ParkId { get; set; }
    public BankTransactionType TransactionType { get; set; }
    public long TotalDebitAmount { get; set; }
    public long TotalCreditAmount { get; set; }
    public int TransactionCount { get; set; }
    [Required(ErrorMessage = "Lý do nhập tay là bắt buộc.")]
    [MaxLength(1000, ErrorMessage = "Lý do nhập tay không được vượt quá 1000 ký tự.")]
    public string ManualReason { get; set; } = string.Empty;
    public int? JobRunItemId { get; set; }
}
