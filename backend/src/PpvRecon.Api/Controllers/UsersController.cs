using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Auth;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Common;
using PpvRecon.Application.Users;
using PpvRecon.Domain.Entities.Identity;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/users")]
public sealed class UsersController(
    PpvReconDbContext dbContext,
    IPasswordHasher passwordHasher,
    IAuditService auditService) : PpvControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserListItemDto>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] string? keyword = null,
        [FromQuery] UserRole? role = null,
        [FromQuery] UserStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query = dbContext.Users.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim().ToUpperInvariant();
            query = query.Where(x =>
                x.NormalizedEmail.Contains(normalizedKeyword)
                || x.FullName.ToUpper().Contains(normalizedKeyword)
                || (x.PhoneNumber != null && x.PhoneNumber.Contains(keyword.Trim())));
        }

        if (role is not null)
        {
            query = query.Where(x => x.Role == role);
        }

        if (status is not null)
        {
            query = query.Where(x => x.Status == status);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.Email)
            .Skip((page - 1) * PagedResult<UserListItemDto>.FixedPageSize)
            .Take(PagedResult<UserListItemDto>.FixedPageSize)
            .Select(x => new UserListItemDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Role = x.Role,
                Status = x.Status,
                FailedLoginCount = x.FailedLoginCount,
                LockedAtUtc = x.LockedAtUtc,
                LockReason = x.LockReason,
                LastLoginAtUtc = x.LastLoginAtUtc,
                CreatedAtUtc = x.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<UserListItemDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<UserListItemDto>.FixedPageSize),
        };

        return Ok(ApiResponse<PagedResult<UserListItemDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserListItemDto>>> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var passwordError = PasswordPolicy.Validate(request.Password);
        if (passwordError is not null)
        {
            return BadRequest(ApiResponse<UserListItemDto>.Fail(passwordError));
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var exists = await dbContext.Users
            .AnyAsync(x => x.NormalizedEmail == normalizedEmail && !x.IsDeleted, cancellationToken);

        if (exists)
        {
            return Conflict(ApiResponse<UserListItemDto>.Fail("Email đã tồn tại."));
        }

        var nowUtc = DateTime.UtcNow;
        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            PasswordHash = passwordHasher.HashPassword(request.Password),
            Role = request.Role,
            Status = UserStatus.Active,
            FailedLoginCount = 0,
            PasswordChangedAtUtc = nowUtc,
            CreatedAtUtc = nowUtc,
            CreatedByUserId = CurrentUserId,
            IsDeleted = false,
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Settings",
            EntityName = "User",
            EntityId = user.Id.ToString(),
            Action = AuditAction.Create,
            After = ToUserListItem(user),
        }, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ApiResponse<UserListItemDto>.Ok(ToUserListItem(user), "Tạo người dùng thành công."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserListItemDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (user is null)
        {
            return NotFound(ApiResponse<UserListItemDto>.Fail("Không tìm thấy người dùng."));
        }

        return Ok(ApiResponse<UserListItemDto>.Ok(ToUserListItem(user)));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserListItemDto>>> Update(
        int id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (user is null)
        {
            return NotFound(ApiResponse<UserListItemDto>.Fail("Không tìm thấy người dùng."));
        }

        if (id == CurrentUserId && (request.Role != user.Role || request.Status != UserStatus.Active))
        {
            return BadRequest(ApiResponse<UserListItemDto>.Fail("Bạn không thể tự đổi role hoặc vô hiệu hóa chính mình."));
        }

        var before = ToUserListItem(user);
        var nowUtc = DateTime.UtcNow;
        user.FullName = request.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        user.Role = request.Role;
        user.Status = request.Status;
        user.UpdatedAtUtc = nowUtc;
        user.UpdatedByUserId = CurrentUserId;

        if (request.Status == UserStatus.Active)
        {
            user.FailedLoginCount = 0;
            user.LockedAtUtc = null;
            user.LockReason = null;
        }
        else
        {
            if (request.Status == UserStatus.Locked)
            {
                user.LockedAtUtc ??= nowUtc;
                user.LockReason ??= "Khóa bởi Admin.";
            }

            await RevokeActiveSessionsAsync(user.Id, nowUtc, CurrentUserId, $"StatusChangedTo{request.Status}", cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Settings",
            EntityName = "User",
            EntityId = user.Id.ToString(),
            Action = AuditAction.Update,
            Before = before,
            After = ToUserListItem(user),
        }, cancellationToken);

        return Ok(ApiResponse<UserListItemDto>.Ok(ToUserListItem(user), "Cập nhật người dùng thành công."));
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ResetPassword(
        int id,
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var passwordError = PasswordPolicy.Validate(request.NewPassword);
        if (passwordError is not null)
        {
            return BadRequest(ApiResponse<object?>.Fail(passwordError));
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (user is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy người dùng."));
        }

        var before = ToUserListItem(user);
        var nowUtc = DateTime.UtcNow;
        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        user.PasswordChangedAtUtc = nowUtc;
        user.FailedLoginCount = 0;
        user.Status = UserStatus.Active;
        user.LockedAtUtc = null;
        user.LockReason = null;
        user.UpdatedAtUtc = nowUtc;
        user.UpdatedByUserId = CurrentUserId;

        await RevokeActiveSessionsAsync(user.Id, nowUtc, CurrentUserId, "PasswordReset", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Settings",
            EntityName = "User",
            EntityId = user.Id.ToString(),
            Action = AuditAction.ResetPassword,
            Before = before,
            After = ToUserListItem(user),
        }, cancellationToken);

        return Ok(ApiResponse<object?>.Ok(null, "Đặt lại mật khẩu thành công."));
    }

    [HttpPost("{id:int}/unlock")]
    public async Task<ActionResult<ApiResponse<object?>>> Unlock(int id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (user is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy người dùng."));
        }

        var before = ToUserListItem(user);
        user.Status = UserStatus.Active;
        user.FailedLoginCount = 0;
        user.LockedAtUtc = null;
        user.LockReason = null;
        user.UpdatedAtUtc = DateTime.UtcNow;
        user.UpdatedByUserId = CurrentUserId;
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Settings",
            EntityName = "User",
            EntityId = user.Id.ToString(),
            Action = AuditAction.UnlockUser,
            Before = before,
            After = ToUserListItem(user),
        }, cancellationToken);

        return Ok(ApiResponse<object?>.Ok(null, "Mở khóa người dùng thành công."));
    }

    [HttpPost("{id:int}/disable")]
    public async Task<ActionResult<ApiResponse<object?>>> Disable(int id, CancellationToken cancellationToken)
    {
        if (id == CurrentUserId)
        {
            return BadRequest(ApiResponse<object?>.Fail("Bạn không thể vô hiệu hóa chính mình."));
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (user is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy người dùng."));
        }

        var before = ToUserListItem(user);
        var nowUtc = DateTime.UtcNow;
        user.Status = UserStatus.Inactive;
        user.UpdatedAtUtc = nowUtc;
        user.UpdatedByUserId = CurrentUserId;
        await RevokeActiveSessionsAsync(user.Id, nowUtc, CurrentUserId, "DisabledByAdmin", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Settings",
            EntityName = "User",
            EntityId = user.Id.ToString(),
            Action = AuditAction.Update,
            Before = before,
            After = ToUserListItem(user),
        }, cancellationToken);

        return Ok(ApiResponse<object?>.Ok(null, "Vô hiệu hóa người dùng thành công."));
    }

    [HttpPost("{id:int}/enable")]
    public async Task<ActionResult<ApiResponse<object?>>> Enable(int id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (user is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy người dùng."));
        }

        var before = ToUserListItem(user);
        user.Status = UserStatus.Active;
        user.FailedLoginCount = 0;
        user.LockedAtUtc = null;
        user.LockReason = null;
        user.UpdatedAtUtc = DateTime.UtcNow;
        user.UpdatedByUserId = CurrentUserId;
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Settings",
            EntityName = "User",
            EntityId = user.Id.ToString(),
            Action = AuditAction.Update,
            Before = before,
            After = ToUserListItem(user),
        }, cancellationToken);

        return Ok(ApiResponse<object?>.Ok(null, "Kích hoạt người dùng thành công."));
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

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
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
