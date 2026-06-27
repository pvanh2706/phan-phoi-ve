using System.ComponentModel.DataAnnotations;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Application.Users;

public sealed class UpdateMyProfileRequest
{
    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [MaxLength(200, ErrorMessage = "Họ tên không được vượt quá 200 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(30, ErrorMessage = "Số điện thoại không được vượt quá 30 ký tự.")]
    public string? PhoneNumber { get; set; }
}

public sealed class ChangePasswordRequest
{
    [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class UserSessionDto
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public string? CreatedByIp { get; set; }
    public string? LastUsedIp { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceName { get; set; }
    public DateTime? LastUsedAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokeReason { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrent { get; set; }
}

public sealed class UpdateMyPreferenceRequest
{
    public ThemeMode ThemeMode { get; set; } = ThemeMode.System;

    [MaxLength(20, ErrorMessage = "Ngôn ngữ không được vượt quá 20 ký tự.")]
    public string? Language { get; set; }
}

public sealed class UserPreferenceDto
{
    public ThemeMode ThemeMode { get; set; }
    public string? Language { get; set; }
}
