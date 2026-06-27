namespace PpvRecon.Domain.Entities.Identity;

public sealed class NotificationPreference
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public bool EnableInApp { get; set; } = true;
    public bool EnableEmail { get; set; } = true;
    public bool EnableSound { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
