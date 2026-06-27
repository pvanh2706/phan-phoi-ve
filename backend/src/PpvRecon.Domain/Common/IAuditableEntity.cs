namespace PpvRecon.Domain.Common;

public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    int? CreatedByUserId { get; set; }
    DateTime? UpdatedAtUtc { get; set; }
    int? UpdatedByUserId { get; set; }
}
