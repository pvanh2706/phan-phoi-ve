using PpvRecon.Domain.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Summaries;

/// <summary>
/// Chi tiết giá vốn vé bán theo từng dòng đặt vé (line-level), phục vụ màn "Chi tiết giá vốn vé bán".
/// Các thông tin đại lý/khu vui chơi được lưu dạng snapshot vì nguồn dữ liệu là hệ thống inventory bên ngoài.
/// </summary>
public sealed class TicketSaleCostDetail : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>Ngày đặt vé (dùng cho lọc và nhóm theo ngày).</summary>
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
    public int? SourceJobRunId { get; set; }
    public int? SourceJobRunItemId { get; set; }
    public int? RawResponseId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
}
