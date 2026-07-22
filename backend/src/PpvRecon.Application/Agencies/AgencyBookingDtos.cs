using PpvRecon.Domain.Enums;

namespace PpvRecon.Application.Agencies;

/// <summary>Một dòng giao dịch đại lý trên TA trả về cho giao diện "Giao dịch của các đại lý trên TA".</summary>
public sealed class AgencyBookingDto
{
    public int Id { get; set; }

    public string BookingCode { get; set; } = string.Empty;
    public DateOnly BookingDate { get; set; }

    public int? BuyingAgentId { get; set; }
    public bool IsAgencyMatched { get; set; }
    public string BuyingAgentCode { get; set; } = string.Empty;
    public string? BuyingAgentName { get; set; }

    public string? ParentBuyingAgentCode { get; set; }
    public string? ParentBuyingAgentName { get; set; }
    public string? SellingAgentCode { get; set; }
    public string? SellingAgentName { get; set; }

    public string? ParkExternalCode { get; set; }
    public string? ParkExternalName { get; set; }
    public string? TicketTypeCode { get; set; }
    public string? TicketTypeName { get; set; }
    public string? TicketGroupName { get; set; }

    public int Quantity { get; set; }
    public long UnitPrice { get; set; }
    public long SalesAmount { get; set; }
    public long CostAmount { get; set; }
    public long Subtotal { get; set; }
    public long Discount { get; set; }

    public string SourceSystem { get; set; } = string.Empty;
    public string SourceTransactionId { get; set; } = string.Empty;
    public SourceType SourceType { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>Ngày đồng bộ (§8) = thời điểm hệ thống lưu/cập nhật bản ghi = UpdatedAtUtc ?? CreatedAtUtc.</summary>
    public DateTime SyncedAtUtc { get; set; }
}

/// <summary>Kết quả trang danh sách kèm tổng số tiền (§12).</summary>
public sealed class AgencyBookingListResult
{
    public IReadOnlyList<AgencyBookingDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    /// <summary>Tổng "Số tiền" (TienBan) trên toàn bộ tập kết quả đã lọc, không chỉ trang hiện tại.</summary>
    public long TotalAmount { get; set; }
}

/// <summary>Kết quả một lần đồng bộ "Giao dịch đại lý trên TA" từ OneInventory (phục vụ log §11 + thông báo UI).</summary>
public sealed class AgencyBookingSyncResult
{
    public DateOnly BusinessDate { get; set; }

    /// <summary>Tổng số bản ghi API trả về (trước khi lọc theo mã đại lý cấp trên).</summary>
    public int TotalLines { get; set; }

    /// <summary>Số bản ghi thỏa điều kiện MaDaiLyMuaCapTren = ParentAgencyCode.</summary>
    public int MatchedParent { get; set; }

    /// <summary>Số bản ghi thêm mới.</summary>
    public int Inserted { get; set; }

    /// <summary>Số bản ghi cập nhật do dữ liệu nguồn thay đổi.</summary>
    public int Updated { get; set; }

    /// <summary>Số bản ghi bỏ qua do dữ liệu không đổi.</summary>
    public int Skipped { get; set; }

    /// <summary>Số bản ghi chưa map được đại lý nội bộ.</summary>
    public int UnmatchedAgency { get; set; }

    /// <summary>Các mã đại lý mua chưa có trong hệ thống (để cảnh báo bổ sung đại lý).</summary>
    public List<string> UnmatchedAgencyCodes { get; set; } = new();

    /// <summary>Mã đại lý cấp trên dùng để lọc (ghi lại cho minh bạch/đối soát).</summary>
    public string ParentAgencyCode { get; set; } = string.Empty;
}
