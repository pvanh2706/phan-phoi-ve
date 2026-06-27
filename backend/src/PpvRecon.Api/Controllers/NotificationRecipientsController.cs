using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Common;
using PpvRecon.Application.Notifications;
using PpvRecon.Domain.Entities.Settings;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/notification-recipients")]
public sealed class NotificationRecipientsController(PpvReconDbContext dbContext) : PpvControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationRecipientDto>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] NotificationType? notificationType = null,
        [FromQuery] bool? active = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query = dbContext.NotificationRecipients.AsNoTracking();

        if (notificationType is not null)
        {
            query = query.Where(x => x.NotificationType == notificationType);
        }

        if (active is not null)
        {
            query = query.Where(x => x.IsActive == active);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.NotificationType)
            .ThenBy(x => x.Email)
            .Skip((page - 1) * PagedResult<NotificationRecipientDto>.FixedPageSize)
            .Take(PagedResult<NotificationRecipientDto>.FixedPageSize)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<NotificationRecipientDto>>.Ok(new PagedResult<NotificationRecipientDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<NotificationRecipientDto>.FixedPageSize),
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<NotificationRecipientDto>>> Create(
        CreateNotificationRecipientRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var exists = await dbContext.NotificationRecipients.AnyAsync(
            x => x.NotificationType == request.NotificationType && x.Email.ToUpper() == email.ToUpperInvariant(),
            cancellationToken);

        if (exists)
        {
            return Conflict(ApiResponse<NotificationRecipientDto>.Fail("Email nhận thông báo đã tồn tại cho loại thông báo này."));
        }

        var recipient = new NotificationRecipient
        {
            NotificationType = request.NotificationType,
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName.Trim(),
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = CurrentUserId,
        };

        dbContext.NotificationRecipients.Add(recipient);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = recipient.Id }, ApiResponse<NotificationRecipientDto>.Ok(ToDto(recipient), "Tạo người nhận thông báo thành công."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<NotificationRecipientDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var recipient = await dbContext.NotificationRecipients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (recipient is null)
        {
            return NotFound(ApiResponse<NotificationRecipientDto>.Fail("Không tìm thấy người nhận thông báo."));
        }

        return Ok(ApiResponse<NotificationRecipientDto>.Ok(ToDto(recipient)));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<NotificationRecipientDto>>> Update(
        int id,
        UpdateNotificationRecipientRequest request,
        CancellationToken cancellationToken)
    {
        var recipient = await dbContext.NotificationRecipients.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (recipient is null)
        {
            return NotFound(ApiResponse<NotificationRecipientDto>.Fail("Không tìm thấy người nhận thông báo."));
        }

        var email = request.Email.Trim();
        var exists = await dbContext.NotificationRecipients.AnyAsync(
            x => x.Id != id && x.NotificationType == request.NotificationType && x.Email.ToUpper() == email.ToUpperInvariant(),
            cancellationToken);

        if (exists)
        {
            return Conflict(ApiResponse<NotificationRecipientDto>.Fail("Email nhận thông báo đã tồn tại cho loại thông báo này."));
        }

        recipient.NotificationType = request.NotificationType;
        recipient.Email = email;
        recipient.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName.Trim();
        recipient.IsActive = request.IsActive;
        recipient.UpdatedAtUtc = DateTime.UtcNow;
        recipient.UpdatedByUserId = CurrentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<NotificationRecipientDto>.Ok(ToDto(recipient), "Cập nhật người nhận thông báo thành công."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(int id, CancellationToken cancellationToken)
    {
        var recipient = await dbContext.NotificationRecipients.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (recipient is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy người nhận thông báo."));
        }

        recipient.IsActive = false;
        recipient.UpdatedAtUtc = DateTime.UtcNow;
        recipient.UpdatedByUserId = CurrentUserId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<object?>.Ok(null, "Đã ngừng gửi thông báo cho email này."));
    }

    private static NotificationRecipientDto ToDto(NotificationRecipient recipient)
    {
        return new NotificationRecipientDto
        {
            Id = recipient.Id,
            NotificationType = recipient.NotificationType,
            Email = recipient.Email,
            DisplayName = recipient.DisplayName,
            IsActive = recipient.IsActive,
            CreatedAtUtc = recipient.CreatedAtUtc,
            UpdatedAtUtc = recipient.UpdatedAtUtc,
        };
    }
}
