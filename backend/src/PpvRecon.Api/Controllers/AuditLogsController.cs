using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Common;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/audit-logs")]
public sealed class AuditLogsController(PpvReconDbContext dbContext) : PpvControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLogDto>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] string? module = null,
        [FromQuery] string? entityName = null,
        [FromQuery] string? entityId = null,
        [FromQuery] int? userId = null,
        [FromQuery] AuditAction? action = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query = dbContext.AuditLogs.AsNoTracking();

        if (!User.IsInRole(nameof(UserRole.Admin)))
        {
            query = query.Where(x => x.Module == "Park");
        }

        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(x => x.Module == module.Trim());
        }

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(x => x.EntityName == entityName.Trim());
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            query = query.Where(x => x.EntityId == entityId.Trim());
        }

        if (userId is not null)
        {
            query = query.Where(x => x.UserId == userId);
        }

        if (action is not null)
        {
            query = query.Where(x => x.Action == action);
        }

        if (fromDate is not null)
        {
            query = query.Where(x => x.OccurredAtUtc >= fromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        if (toDate is not null)
        {
            query = query.Where(x => x.OccurredAtUtc < toDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .Skip((page - 1) * PagedResult<AuditLogDto>.FixedPageSize)
            .Take(PagedResult<AuditLogDto>.FixedPageSize)
            .Select(x => new AuditLogDto
            {
                Id = x.Id,
                OccurredAtUtc = x.OccurredAtUtc,
                UserId = x.UserId,
                UserEmailSnapshot = x.UserEmailSnapshot,
                UserRoleSnapshot = x.UserRoleSnapshot,
                Module = x.Module,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Action = x.Action,
                BeforeJson = x.BeforeJson,
                AfterJson = x.AfterJson,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                CorrelationId = x.CorrelationId,
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<AuditLogDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<AuditLogDto>.FixedPageSize),
        };

        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(result));
    }
}
