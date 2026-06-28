using PpvRecon.Domain.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Summaries;

/// <summary>
/// Chi tiết giao dịch ngân hàng theo từng dòng (line-level), phục vụ màn "Danh sách nạp tiền KVC theo ngày".
/// Mỗi dòng là một giao dịch nạp tiền (Prepaid) hoặc thanh toán công nợ (Debt) lấy từ sao kê ngân hàng.
/// </summary>
public sealed class BankTransactionDetail : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>Ngày giao dịch (dùng để lọc và nhóm theo ngày).</summary>
    public DateOnly BusinessDate { get; set; }

    /// <summary>Thời điểm giao dịch đầy đủ (ngày giờ).</summary>
    public DateTime TransactionAtUtc { get; set; }

    public ParkPaymentType PaymentType { get; set; }

    /// <summary>Ghi nợ.</summary>
    public long DebitAmount { get; set; }

    /// <summary>Ghi có.</summary>
    public long CreditAmount { get; set; }

    /// <summary>Nội dung giao dịch (memo từ sao kê).</summary>
    public string Content { get; set; } = string.Empty;

    public string? BankAccount { get; set; }
    public int? ParkId { get; set; }

    public SourceType SourceType { get; set; }
    public int? SourceJobRunId { get; set; }
    public int? SourceJobRunItemId { get; set; }
    public int? RawResponseId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
}
