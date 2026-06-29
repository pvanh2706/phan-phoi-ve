using PpvRecon.Domain.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Summaries;

/// <summary>
/// Giao dịch ngân hàng đã gộp theo (KVC, ngày), phục vụ màn "Danh sách nạp tiền KVC theo ngày".
/// Mỗi dòng là tổng hợp các giao dịch nạp tiền (Prepaid) hoặc thanh toán công nợ (Debt) trong ngày
/// của một KVC, lấy từ sao kê ngân hàng (Ghi nợ/Ghi có là tổng cộng dồn).
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

    /// <summary>Nội dung giao dịch (memo từ sao kê). Khi gộp ≥2 GD thì là "Gồm N giao dịch".</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// JSON chi tiết từng giao dịch đã gộp (giờ/nội dung/ghi nợ/ghi có), để xem khi bấm vào.
    /// Null khi dòng chỉ có 1 giao dịch (lúc đó <see cref="Content"/> đã là nội dung gốc).
    /// </summary>
    public string? LineItemsJson { get; set; }

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
