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
    public string? BankAccountSnapshot { get; set; }
    public SourceType SourceType { get; set; }
    public int? SourceJobRunId { get; set; }
    public int? SourceJobRunItemId { get; set; }
    public int? RawResponseId { get; set; }
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

public sealed class TicketSaleCostDetailDto
{
    public int Id { get; set; }
    public DateOnly BusinessDate { get; set; }
    public int? ParkId { get; set; }
    public ParkPaymentType PaymentType { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public long UnitPrice { get; set; }
    public string TicketTypeName { get; set; } = string.Empty;
    public string? TicketGroupName { get; set; }
    public long SalesAmount { get; set; }
    public long CostAmount { get; set; }
    public string? SellingAgentCode { get; set; }
    public int Quantity { get; set; }
    public string? BuyingAgentCode { get; set; }
    public string? BuyingAgentName { get; set; }
    public string ParkCodeSnapshot { get; set; } = string.Empty;
    public string ParkNameSnapshot { get; set; } = string.Empty;
    public long Subtotal { get; set; }
    public string? ExternalLineId { get; set; }
    public string? SellingAgentName { get; set; }
    public string? TicketTypeCode { get; set; }
    public string? ParentBuyingAgentName { get; set; }
    public string? ParentBuyingAgentCode { get; set; }
    public SourceType SourceType { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class BankTransactionDetailDto
{
    public int Id { get; set; }
    public DateOnly BusinessDate { get; set; }
    public DateTime TransactionAtUtc { get; set; }
    public ParkPaymentType PaymentType { get; set; }
    public long DebitAmount { get; set; }
    public long CreditAmount { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? BankAccount { get; set; }
    public int? ParkId { get; set; }
    public string? ParkName { get; set; }
    public SourceType SourceType { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>Chi tiết từng giao dịch khi dòng này gộp ≥2 GD; null nếu chỉ có 1 GD.</summary>
    public List<BankTransactionLineItemDto>? LineItems { get; set; }
}

/// <summary>Một giao dịch thành phần trong dòng tổng hợp theo (KVC, ngày).</summary>
public sealed class BankTransactionLineItemDto
{
    public DateTime TransactionAtUtc { get; set; }
    public string Content { get; set; } = string.Empty;
    public long DebitAmount { get; set; }
    public long CreditAmount { get; set; }
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
