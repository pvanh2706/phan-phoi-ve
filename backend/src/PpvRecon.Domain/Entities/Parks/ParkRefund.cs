using PpvRecon.Domain.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Parks;

public sealed class ParkRefund : IAuditableEntity, ISoftDelete
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public DateOnly RefundDate { get; set; }
    public DateOnly? BusinessDate { get; set; }
    public int? ParkId { get; set; }
    public string? ParkCodeSnapshot { get; set; }
    public string ParkNameSnapshot { get; set; } = string.Empty;
    public ParkPaymentType PaymentType { get; set; }
    public string? TicketTypeCode { get; set; }
    public string? TicketTypeName { get; set; }
    public int Quantity { get; set; }
    public long? ParkRefundAmount { get; set; }
    public long? CustomerRefundAmount { get; set; }
    public string? Reason { get; set; }
    public ParkRefundStatus ParkRefundStatus { get; set; }
    public CustomerRefundStatus CustomerRefundStatus { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public int? DeletedByUserId { get; set; }
}
