using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Settings;

public sealed class NotificationRecipient
{
    public int Id { get; set; }
    public NotificationType NotificationType { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
}
