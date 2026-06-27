using System.ComponentModel.DataAnnotations;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Application.Users;

public sealed class UserListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? LockedAtUtc { get; set; }
    public string? LockReason { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class CreateUserRequest
{
    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [MaxLength(200, ErrorMessage = "Họ tên không được vượt quá 200 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [MaxLength(320, ErrorMessage = "Email không được vượt quá 320 ký tự.")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30, ErrorMessage = "Số điện thoại không được vượt quá 30 ký tự.")]
    public string? PhoneNumber { get; set; }

    public UserRole Role { get; set; } = UserRole.Member;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    public string Password { get; set; } = string.Empty;
}

public sealed class UpdateUserRequest
{
    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [MaxLength(200, ErrorMessage = "Họ tên không được vượt quá 200 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(30, ErrorMessage = "Số điện thoại không được vượt quá 30 ký tự.")]
    public string? PhoneNumber { get; set; }

    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
}

public sealed class ResetPasswordRequest
{
    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
    public string NewPassword { get; set; } = string.Empty;
}
