using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Api.Auth;
using PpvRecon.Application.Auth;
using PpvRecon.Application.Common;
using PpvRecon.Application.Users;
using PpvRecon.Domain.Entities.Identity;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public sealed class MeController(
    PpvReconDbContext dbContext,
    IPasswordHasher passwordHasher) : PpvControllerBase
{
    private const long MaxAvatarBytes = 2 * 1024 * 1024;
    private static readonly HashSet<string> AllowedAvatarExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
    };

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<UserListItemDto>>> GetProfile(CancellationToken cancellationToken)
    {
        var user = await GetCurrentActiveUserAsync(cancellationToken);
        if (user is null)
        {
            return Unauthorized(ApiResponse<UserListItemDto>.Fail("Tài khoản không còn hoạt động."));
        }

        return Ok(ApiResponse<UserListItemDto>.Ok(ToUserListItem(user)));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<UserListItemDto>>> UpdateProfile(
        UpdateMyProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await GetCurrentActiveUserAsync(cancellationToken);
        if (user is null)
        {
            return Unauthorized(ApiResponse<UserListItemDto>.Fail("Tài khoản không còn hoạt động."));
        }

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        user.UpdatedAtUtc = DateTime.UtcNow;
        user.UpdatedByUserId = user.Id;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<UserListItemDto>.Ok(ToUserListItem(user), "Cập nhật thông tin tài khoản thành công."));
    }

    [HttpPost("avatar")]
    [RequestSizeLimit(MaxAvatarBytes)]
    public async Task<ActionResult<ApiResponse<object?>>> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        var user = await GetCurrentActiveUserAsync(cancellationToken);
        if (user is null)
        {
            return Unauthorized(ApiResponse<object?>.Fail("Tài khoản không còn hoạt động."));
        }

        if (file.Length == 0)
        {
            return BadRequest(ApiResponse<object?>.Fail("File avatar không hợp lệ."));
        }

        if (file.Length > MaxAvatarBytes)
        {
            return BadRequest(ApiResponse<object?>.Fail("Avatar không được vượt quá 2MB."));
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedAvatarExtensions.Contains(extension))
        {
            return BadRequest(ApiResponse<object?>.Fail("Avatar chỉ hỗ trợ JPG, PNG hoặc WebP."));
        }

        var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars", user.Id.ToString());
        Directory.CreateDirectory(uploadRoot);

        var fileName = $"avatar_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension.ToLowerInvariant()}";
        var destinationPath = Path.Combine(uploadRoot, fileName);

        await using (var stream = System.IO.File.Create(destinationPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var oldAvatarPath = user.AvatarPath;
        user.AvatarPath = $"/uploads/avatars/{user.Id}/{fileName}";
        user.UpdatedAtUtc = DateTime.UtcNow;
        user.UpdatedByUserId = user.Id;
        await dbContext.SaveChangesAsync(cancellationToken);

        DeleteOldAvatarIfSafe(oldAvatarPath);

        return Ok(ApiResponse<object?>.Ok(new { avatarPath = user.AvatarPath }, "Cập nhật avatar thành công."));
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await GetCurrentActiveUserAsync(cancellationToken);
        if (user is null)
        {
            return Unauthorized(ApiResponse<object?>.Fail("Tài khoản không còn hoạt động."));
        }

        if (!passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(ApiResponse<object?>.Fail("Mật khẩu hiện tại không đúng."));
        }

        var passwordError = PasswordPolicy.Validate(request.NewPassword);
        if (passwordError is not null)
        {
            return BadRequest(ApiResponse<object?>.Fail(passwordError));
        }

        var nowUtc = DateTime.UtcNow;
        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        user.PasswordChangedAtUtc = nowUtc;
        user.FailedLoginCount = 0;
        user.UpdatedAtUtc = nowUtc;
        user.UpdatedByUserId = user.Id;

        var currentSessionId = CurrentSessionId;
        var sessions = await dbContext.UserSessions
            .Where(x => x.UserId == user.Id
                && x.RevokedAtUtc == null
                && x.ExpiresAtUtc > nowUtc
                && x.Id != currentSessionId)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.RevokedAtUtc = nowUtc;
            session.RevokedByUserId = user.Id;
            session.RevokeReason = "PasswordChanged";
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Đổi mật khẩu thành công."));
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserSessionDto>>>> GetSessions(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId is null)
        {
            return Unauthorized(ApiResponse<IReadOnlyList<UserSessionDto>>.Fail("Bạn cần đăng nhập để tiếp tục."));
        }

        var nowUtc = DateTime.UtcNow;
        var currentSessionId = CurrentSessionId;
        var sessions = await dbContext.UserSessions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(100)
            .Select(x => new UserSessionDto
            {
                Id = x.Id,
                CreatedAtUtc = x.CreatedAtUtc,
                ExpiresAtUtc = x.ExpiresAtUtc,
                CreatedByIp = x.CreatedByIp,
                LastUsedIp = x.LastUsedIp,
                UserAgent = x.UserAgent,
                DeviceName = x.DeviceName,
                LastUsedAtUtc = x.LastUsedAtUtc,
                RevokedAtUtc = x.RevokedAtUtc,
                RevokeReason = x.RevokeReason,
                IsActive = x.RevokedAtUtc == null && x.ExpiresAtUtc > nowUtc,
                IsCurrent = x.Id == currentSessionId,
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<UserSessionDto>>.Ok(sessions));
    }

    [HttpPost("sessions/{sessionId:int}/revoke")]
    public async Task<ActionResult<ApiResponse<object?>>> RevokeSession(int sessionId, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId is null)
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để tiếp tục."));
        }

        var session = await dbContext.UserSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, cancellationToken);

        if (session is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy phiên đăng nhập."));
        }

        if (session.RevokedAtUtc is null)
        {
            session.RevokedAtUtc = DateTime.UtcNow;
            session.RevokedByUserId = userId;
            session.RevokeReason = "UserRevoked";
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (session.Id == CurrentSessionId)
        {
            ClearRefreshCookie();
        }

        return Ok(ApiResponse<object?>.Ok(null, "Đã đăng xuất thiết bị."));
    }

    [HttpPost("sessions/revoke-all")]
    public async Task<ActionResult<ApiResponse<object?>>> RevokeAllSessions(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId is null)
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để tiếp tục."));
        }

        var nowUtc = DateTime.UtcNow;
        var sessions = await dbContext.UserSessions
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null && x.ExpiresAtUtc > nowUtc)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.RevokedAtUtc = nowUtc;
            session.RevokedByUserId = userId;
            session.RevokeReason = "UserRevokedAll";
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        ClearRefreshCookie();

        return Ok(ApiResponse<object?>.Ok(null, "Đã đăng xuất tất cả thiết bị."));
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<ApiResponse<UserPreferenceDto>>> GetPreferences(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId is null)
        {
            return Unauthorized(ApiResponse<UserPreferenceDto>.Fail("Bạn cần đăng nhập để tiếp tục."));
        }

        var preference = await dbContext.UserPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        var response = new UserPreferenceDto
        {
            ThemeMode = preference?.ThemeMode ?? ThemeMode.System,
            Language = preference?.Language,
        };

        return Ok(ApiResponse<UserPreferenceDto>.Ok(response));
    }

    [HttpPut("preferences")]
    public async Task<ActionResult<ApiResponse<UserPreferenceDto>>> UpdatePreferences(
        UpdateMyPreferenceRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId is null)
        {
            return Unauthorized(ApiResponse<UserPreferenceDto>.Fail("Bạn cần đăng nhập để tiếp tục."));
        }

        var nowUtc = DateTime.UtcNow;
        var preference = await dbContext.UserPreferences
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (preference is null)
        {
            preference = new UserPreference
            {
                UserId = userId.Value,
                ThemeMode = request.ThemeMode,
                Language = string.IsNullOrWhiteSpace(request.Language) ? null : request.Language.Trim(),
                CreatedAtUtc = nowUtc,
            };
            dbContext.UserPreferences.Add(preference);
        }
        else
        {
            preference.ThemeMode = request.ThemeMode;
            preference.Language = string.IsNullOrWhiteSpace(request.Language) ? null : request.Language.Trim();
            preference.UpdatedAtUtc = nowUtc;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<UserPreferenceDto>.Ok(
            new UserPreferenceDto { ThemeMode = preference.ThemeMode, Language = preference.Language },
            "Cập nhật giao diện thành công."));
    }

    private async Task<User?> GetCurrentActiveUserAsync(CancellationToken cancellationToken)
    {
        if (CurrentUserId is null)
        {
            return null;
        }

        return await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == CurrentUserId && !x.IsDeleted && x.Status == UserStatus.Active, cancellationToken);
    }

    private void ClearRefreshCookie()
    {
        Response.Cookies.Delete(AuthCookieNames.RefreshToken, new CookieOptions
        {
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
        });
    }

    private static void DeleteOldAvatarIfSafe(string? oldAvatarPath)
    {
        if (string.IsNullOrWhiteSpace(oldAvatarPath) || !oldAvatarPath.StartsWith("/uploads/avatars/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relativePath = oldAvatarPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath));
        var avatarRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars"));

        if (fullPath.StartsWith(avatarRoot, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }

    private static UserListItemDto ToUserListItem(User user)
    {
        return new UserListItemDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            Status = user.Status,
            FailedLoginCount = user.FailedLoginCount,
            LockedAtUtc = user.LockedAtUtc,
            LockReason = user.LockReason,
            LastLoginAtUtc = user.LastLoginAtUtc,
            CreatedAtUtc = user.CreatedAtUtc,
        };
    }
}
