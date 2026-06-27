using System.ComponentModel.DataAnnotations;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Application.Notifications;

public sealed class NotificationRecipientDto
{
    public int Id { get; set; }
    public NotificationType NotificationType { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class CreateNotificationRecipientRequest
{
    public NotificationType NotificationType { get; set; } = NotificationType.SyncErrorSummary;

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateNotificationRecipientRequest
{
    public NotificationType NotificationType { get; set; } = NotificationType.SyncErrorSummary;

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    public bool IsActive { get; set; } = true;
}
