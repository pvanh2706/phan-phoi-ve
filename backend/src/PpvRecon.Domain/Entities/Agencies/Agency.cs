using PpvRecon.Domain.Common;

namespace PpvRecon.Domain.Entities.Agencies;

public sealed class Agency : IAuditableEntity, ISoftDelete
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentCode { get; set; }
    public string? ParentName { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public int? DeletedByUserId { get; set; }
}
