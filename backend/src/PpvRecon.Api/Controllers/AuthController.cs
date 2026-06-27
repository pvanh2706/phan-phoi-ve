using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Api.Auth;
using PpvRecon.Application.Auth;
using PpvRecon.Application.Common;
using PpvRecon.Domain.Entities.Identity;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    PpvReconDbContext dbContext,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : ControllerBase
{
    private const string InvalidLoginMessage = "Email hoặc mật khẩu không đúng.";
    private const string ExpiredSessionMessage = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail && !x.IsDeleted, cancellationToken);

        if (user is null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail(InvalidLoginMessage));
        }

        if (user.Status == UserStatus.Inactive)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail("Tài khoản đã bị vô hiệu hóa."));
        }

        if (user.Status == UserStatus.Locked)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail("Tài khoản đang bị khóa. Vui lòng liên hệ Admin."));
        }

        if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.FailedLoginCount += 1;

            if (user.FailedLoginCount >= 3)
            {
                user.Status = UserStatus.Locked;
                user.LockedAtUtc = nowUtc;
                user.LockReason = "Sai mật khẩu 3 lần liên tiếp.";
                await RevokeActiveSessionsAsync(user.Id, nowUtc, null, "LockedAfterFailedLogin", cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);
                return Unauthorized(ApiResponse<LoginResponse>.Fail("Tài khoản đã bị khóa do nhập sai mật khẩu 3 lần. Vui lòng liên hệ Admin."));
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return Unauthorized(ApiResponse<LoginResponse>.Fail(InvalidLoginMessage));
        }

        user.FailedLoginCount = 0;
        user.LockedAtUtc = null;
        user.LockReason = null;
        user.LastLoginAtUtc = nowUtc;

        var refreshToken = tokenService.GenerateRefreshToken(nowUtc);
        var session = new UserSession
        {
            UserId = user.Id,
            RefreshTokenHash = refreshToken.RefreshTokenHash,
            ExpiresAtUtc = refreshToken.ExpiresAtUtc,
            CreatedAtUtc = nowUtc,
            CreatedByIp = GetClientIp(),
            UserAgent = GetUserAgent(),
            DeviceName = Truncate(GetUserAgent(), 300),
        };

        dbContext.UserSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = tokenService.GenerateAccessToken(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            session.Id,
            nowUtc);

        session.JwtId = accessToken.JwtId;
        await dbContext.SaveChangesAsync(cancellationToken);

        AppendRefreshCookie(refreshToken.RefreshToken, refreshToken.ExpiresAtUtc);

        var response = new LoginResponse
        {
            AccessToken = accessToken.AccessToken,
            ExpiresAtUtc = accessToken.ExpiresAtUtc,
            User = ToUserSummary(user),
        };

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Đăng nhập thành công."));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<RefreshResponse>>> Refresh(CancellationToken cancellationToken)
    {
        var rawRefreshToken = Request.Cookies[AuthCookieNames.RefreshToken];
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            ClearRefreshCookie();
            return Unauthorized(ApiResponse<RefreshResponse>.Fail(ExpiredSessionMessage));
        }

        var nowUtc = DateTime.UtcNow;
        var refreshTokenHash = tokenService.HashRefreshToken(rawRefreshToken);
        var session = await dbContext.UserSessions
            .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, cancellationToken);

        if (session is null || session.RevokedAtUtc is not null || session.ExpiresAtUtc <= nowUtc)
        {
            ClearRefreshCookie();
            return Unauthorized(ApiResponse<RefreshResponse>.Fail(ExpiredSessionMessage));
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == session.UserId && !x.IsDeleted, cancellationToken);

        if (user is null || user.Status != UserStatus.Active)
        {
            session.RevokedAtUtc = nowUtc;
            session.RevokeReason = "UserNotActive";
            await dbContext.SaveChangesAsync(cancellationToken);

            ClearRefreshCookie();
            return Unauthorized(ApiResponse<RefreshResponse>.Fail("Phiên đăng nhập không còn hợp lệ."));
        }

        session.LastUsedAtUtc = nowUtc;
        session.LastUsedIp = GetClientIp();
        session.RevokedAtUtc = nowUtc;
        session.RevokeReason = "Rotated";

        var newRefreshToken = tokenService.GenerateRefreshToken(nowUtc);
        var newSession = new UserSession
        {
            UserId = user.Id,
            RefreshTokenHash = newRefreshToken.RefreshTokenHash,
            ExpiresAtUtc = newRefreshToken.ExpiresAtUtc,
            CreatedAtUtc = nowUtc,
            CreatedByIp = GetClientIp(),
            UserAgent = GetUserAgent(),
            DeviceName = Truncate(GetUserAgent(), 300),
            LastUsedAtUtc = nowUtc,
            LastUsedIp = GetClientIp(),
        };

        dbContext.UserSessions.Add(newSession);
        await dbContext.SaveChangesAsync(cancellationToken);

        session.ReplacedBySessionId = newSession.Id;
        var accessToken = tokenService.GenerateAccessToken(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            newSession.Id,
            nowUtc);

        newSession.JwtId = accessToken.JwtId;
        await dbContext.SaveChangesAsync(cancellationToken);

        AppendRefreshCookie(newRefreshToken.RefreshToken, newRefreshToken.ExpiresAtUtc);

        var response = new RefreshResponse
        {
            AccessToken = accessToken.AccessToken,
            ExpiresAtUtc = accessToken.ExpiresAtUtc,
            User = ToUserSummary(user),
        };

        return Ok(ApiResponse<RefreshResponse>.Ok(response, "Làm mới phiên đăng nhập thành công."));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object?>>> Logout(CancellationToken cancellationToken)
    {
        var rawRefreshToken = Request.Cookies[AuthCookieNames.RefreshToken];
        if (!string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            var refreshTokenHash = tokenService.HashRefreshToken(rawRefreshToken);
            var session = await dbContext.UserSessions
                .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, cancellationToken);

            if (session is not null && session.RevokedAtUtc is null)
            {
                session.RevokedAtUtc = DateTime.UtcNow;
                session.RevokedByUserId = GetCurrentUserIdOrNull();
                session.RevokeReason = "Logout";
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        ClearRefreshCookie();
        return Ok(ApiResponse<object?>.Ok(null, "Đăng xuất thành công."));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<MeResponse>>> Me(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserIdOrNull();
        if (userId is null)
        {
            return Unauthorized(ApiResponse<MeResponse>.Fail("Bạn cần đăng nhập để tiếp tục."));
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);

        if (user is null || user.Status != UserStatus.Active)
        {
            return Unauthorized(ApiResponse<MeResponse>.Fail("Tài khoản không còn hoạt động."));
        }

        return Ok(ApiResponse<MeResponse>.Ok(new MeResponse { User = ToUserSummary(user) }));
    }

    private async Task RevokeActiveSessionsAsync(
        int userId,
        DateTime nowUtc,
        int? revokedByUserId,
        string reason,
        CancellationToken cancellationToken)
    {
        var sessions = await dbContext.UserSessions
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null && x.ExpiresAtUtc > nowUtc)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.RevokedAtUtc = nowUtc;
            session.RevokedByUserId = revokedByUserId;
            session.RevokeReason = reason;
        }
    }

    private void AppendRefreshCookie(string refreshToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(AuthCookieNames.RefreshToken, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = expiresAtUtc,
            Path = "/api/auth",
        });
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

    private int? GetCurrentUserIdOrNull()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private string? GetClientIp()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        return string.IsNullOrWhiteSpace(userAgent) ? null : userAgent;
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
    }

    private static string? Truncate(string? value, int maxLength)
    {
        return value is null || value.Length <= maxLength ? value : value[..maxLength];
    }

    private static UserSummaryDto ToUserSummary(User user)
    {
        return new UserSummaryDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            Status = user.Status,
            AvatarPath = user.AvatarPath,
        };
    }
}
