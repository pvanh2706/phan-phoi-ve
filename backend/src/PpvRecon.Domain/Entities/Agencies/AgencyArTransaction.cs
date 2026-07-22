using PpvRecon.Domain.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Agencies;

/// <summary>
/// Một giao dịch "Thanh toán tiền cho booking" của đại lý trên hệ thống AR (ar.ezcloud.vn), trích từ file
/// Excel "Lịch sử biến động số dư". Phục vụ màn "Giao dịch của các đại lý trên AR".
/// Chống trùng theo <see cref="DedupHash"/> (băm từ các trường định danh) nên job chạy lại cùng ngày sẽ
/// upsert, không tạo bản ghi trùng.
/// </summary>
public sealed class AgencyArTransaction : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>Mã booking trích từ mô tả — lưu chuỗi để giữ số 0 ở đầu nếu có.</summary>
    public string BookingId { get; set; } = string.Empty;

    /// <summary>Tên đại lý (cột __EMPTY_1).</summary>
    public string? TravelAgentName { get; set; }

    /// <summary>Mã đại lý (cột __EMPTY).</summary>
    public string? TravelAgentCode { get; set; }

    /// <summary>Mã tài khoản công nợ (cột __EMPTY_2).</summary>
    public string? ReceivableAccountCode { get; set; }

    /// <summary>
    /// Thời điểm giao dịch trên AR — lưu giờ tường (wall-clock) Việt Nam dạng DateTime, không quy đổi UTC
    /// để tránh lệch ±7h (§10). API gắn offset +07:00 khi trả về. Dùng DateTime (không DateTimeOffset) vì
    /// SQLite không hỗ trợ ORDER BY trên DateTimeOffset.
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>Ngày nghiệp vụ (giờ VN) để lọc nhanh theo khoảng ngày.</summary>
    public DateOnly BusinessDate { get; set; }

    /// <summary>Số tiền giao dịch, luôn lưu số dương (VND, kiểu long — không dùng float/double) — |__EMPTY_4|.</summary>
    public long Amount { get; set; }

    /// <summary>Nội dung mô tả gốc (cột __EMPTY_7).</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Khóa chống trùng (§13) = SHA-256 của chuỗi chuẩn hóa
    /// BookingId | TransactionDate | Amount | TravelAgentCode | ReceivableAccountCode.
    /// </summary>
    public string DedupHash { get; set; } = string.Empty;

    /// <summary>Số dòng trong file Excel nguồn (audit/truy vết).</summary>
    public int? SourceRowNumber { get; set; }

    public SourceType SourceType { get; set; }
    public int? SourceJobRunId { get; set; }
    public int? SourceJobRunItemId { get; set; }

    // Audit ("SyncDate" = thời điểm đồng bộ = UpdatedAtUtc ?? CreatedAtUtc).
    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
}
