using PpvRecon.Domain.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Parks;

public sealed class ParkTicketType : IAuditableEntity, ISoftDelete
{
    public int Id { get; set; }
    public int ParkId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string TicketTypeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TicketGroupName { get; set; }
    public long CostPrice { get; set; }
    public RecordStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public int? DeletedByUserId { get; set; }
}
