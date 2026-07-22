using PpvRecon.Domain.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Agencies;

/// <summary>
/// Một dòng giao dịch (booking) của đại lý ghi nhận trên hệ thống TA (OneInventory), map 1:1 với một
/// phần tử trong rp_booking_list. Phục vụ màn "Giao dịch của các đại lý trên TA".
/// Chống trùng theo (SourceSystem + SourceTransactionId) nên job chạy lại cùng ngày sẽ upsert, không tạo trùng.
/// Thông tin đại lý/khu vui chơi lưu dạng snapshot vì nguồn là hệ thống bên ngoài; đồng thời liên kết nội bộ
/// tới đại lý (BuyingAgentId) khi map được MaDaiLyMua với Agency.Code.
/// </summary>
public sealed class AgencyBooking : IAuditableEntity
{
    public int Id { get; set; }

    // ── Định danh nguồn (chống trùng §10) ──

    /// <summary>Hệ thống nguồn, cố định "OneInventory".</summary>
    public string SourceSystem { get; set; } = "OneInventory";

    /// <summary>ID giao dịch nguồn (OneInventory field "ID").</summary>
    public string SourceTransactionId { get; set; } = string.Empty;

    // ── Thông tin hiển thị chính ──

    /// <summary>Mã booking (MaDatVe).</summary>
    public string BookingCode { get; set; } = string.Empty;

    /// <summary>Ngày đặt (NgayDat) — ngày nghiệp vụ để lọc/nhóm/hiển thị.</summary>
    public DateOnly BookingDate { get; set; }

    // ── Liên kết đại lý mua (§9) ──

    /// <summary>Khóa nội bộ của đại lý mua (Agency.Id); null nếu chưa map được.</summary>
    public int? BuyingAgentId { get; set; }

    /// <summary>Đã map được đại lý mua nội bộ hay chưa (phục vụ cảnh báo/đối soát).</summary>
    public bool IsAgencyMatched { get; set; }

    /// <summary>Mã đại lý mua nguồn (MaDaiLyMua) — luôn giữ để đối soát dù có map được hay không.</summary>
    public string BuyingAgentCode { get; set; } = string.Empty;

    /// <summary>Tên đại lý mua (TenDaiLyMua).</summary>
    public string? BuyingAgentName { get; set; }

    // ── Đại lý cấp trên / đại lý bán ──

    /// <summary>Mã đại lý mua cấp trên (MaDaiLyMuaCapTren) — điều kiện lọc §7.</summary>
    public string? ParentBuyingAgentCode { get; set; }

    /// <summary>Tên đại lý mua cấp trên (TenDaiLyMuaCapTren).</summary>
    public string? ParentBuyingAgentName { get; set; }

    /// <summary>Mã đại lý bán (MaDaiLyBan).</summary>
    public string? SellingAgentCode { get; set; }

    /// <summary>Tên đại lý bán (TenDaiLyBan).</summary>
    public string? SellingAgentName { get; set; }

    // ── Khu vui chơi / loại vé (snapshot từ nguồn) ──

    /// <summary>Mã khu vui chơi (MaKhuVuiChoi).</summary>
    public string? ParkExternalCode { get; set; }

    /// <summary>Tên khu vui chơi (TenKhuVuiChoi).</summary>
    public string? ParkExternalName { get; set; }

    /// <summary>Mã loại vé (MaLoaiVe).</summary>
    public string? TicketTypeCode { get; set; }

    /// <summary>Tên loại vé (TenLoaiVe).</summary>
    public string? TicketTypeName { get; set; }

    /// <summary>Tên nhóm loại vé (TenNhomLoaiVe).</summary>
    public string? TicketGroupName { get; set; }

    // ── Số lượng & tiền (VND, kiểu long — không dùng float/double §6) ──

    /// <summary>Số lượng vé (SoLuongVe).</summary>
    public int Quantity { get; set; }

    /// <summary>Đơn giá (DonGia).</summary>
    public long UnitPrice { get; set; }

    /// <summary>Tiền bán (TienBan) — cột "Số tiền" hiển thị trên giao diện.</summary>
    public long SalesAmount { get; set; }

    /// <summary>Tiền vốn (TienVon).</summary>
    public long CostAmount { get; set; }

    /// <summary>Tạm tính (TamTinh).</summary>
    public long Subtotal { get; set; }

    /// <summary>Giảm giá (GiamGia).</summary>
    public long Discount { get; set; }

    // ── Nguồn gốc / provenance ──

    public SourceType SourceType { get; set; }
    public int? SourceJobRunId { get; set; }
    public int? SourceJobRunItemId { get; set; }
    public int? RawResponseId { get; set; }

    // ── Audit ("Ngày đồng bộ" §8 = thời điểm hệ thống lưu: UpdatedAtUtc ?? CreatedAtUtc) ──

    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
}
