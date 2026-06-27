namespace PpvRecon.Domain.Common;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAtUtc { get; set; }
    int? DeletedByUserId { get; set; }
}
