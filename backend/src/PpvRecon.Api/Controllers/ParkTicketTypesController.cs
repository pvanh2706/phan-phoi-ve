using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Common;
using PpvRecon.Application.Parks;
using PpvRecon.Domain.Entities.Parks;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/park-ticket-types")]
public sealed class ParkTicketTypesController(
    PpvReconDbContext dbContext,
    IAuditService auditService) : PpvControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ParkTicketTypeDto>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int? parkId = null,
        [FromQuery] string? keyword = null,
        [FromQuery] ParkPaymentType? paymentType = null,
        [FromQuery] RecordStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);

        var query =
            from ticketType in dbContext.ParkTicketTypes.AsNoTracking()
            join park in dbContext.Parks.AsNoTracking() on ticketType.ParkId equals park.Id
            where !ticketType.IsDeleted && !park.IsDeleted
            select new { ticketType, park };

        if (parkId is not null)
        {
            query = query.Where(x => x.ticketType.ParkId == parkId);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalized = keyword.Trim().ToUpperInvariant();
            query = query.Where(x =>
                x.ticketType.Code.ToUpper().Contains(normalized)
                || x.ticketType.TicketTypeCode.ToUpper().Contains(normalized)
                || x.ticketType.Name.ToUpper().Contains(normalized)
                || x.park.Code.ToUpper().Contains(normalized)
                || x.park.Name.ToUpper().Contains(normalized));
        }

        if (paymentType is not null)
        {
            query = query.Where(x => x.park.PaymentType == paymentType);
        }

        if (status is not null)
        {
            query = query.Where(x => x.ticketType.Status == status);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.park.Name)
            .ThenBy(x => x.ticketType.Name)
            .Skip((page - 1) * PagedResult<ParkTicketTypeDto>.FixedPageSize)
            .Take(PagedResult<ParkTicketTypeDto>.FixedPageSize)
            .Select(x => new ParkTicketTypeDto
            {
                Id = x.ticketType.Id,
                ParkId = x.ticketType.ParkId,
                ParkCode = x.park.Code,
                ParkName = x.park.Name,
                PaymentType = x.park.PaymentType,
                Code = x.ticketType.Code,
                TicketTypeCode = x.ticketType.TicketTypeCode,
                Name = x.ticketType.Name,
                TicketGroupName = x.ticketType.TicketGroupName,
                CostPrice = x.ticketType.CostPrice,
                Status = x.ticketType.Status,
                IsDeleted = x.ticketType.IsDeleted,
                CreatedAtUtc = x.ticketType.CreatedAtUtc,
                UpdatedAtUtc = x.ticketType.UpdatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<ParkTicketTypeDto>>.Ok(new PagedResult<ParkTicketTypeDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<ParkTicketTypeDto>.FixedPageSize),
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ParkTicketTypeDto>>> Create(
        CreateParkTicketTypeRequest request,
        CancellationToken cancellationToken)
    {
        if (request.CostPrice < 0)
        {
            return BadRequest(ApiResponse<ParkTicketTypeDto>.Fail("Giá vốn không được âm."));
        }

        var park = await dbContext.Parks.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ParkId && !x.IsDeleted, cancellationToken);
        if (park is null)
        {
            return BadRequest(ApiResponse<ParkTicketTypeDto>.Fail("Khu vui chơi không tồn tại."));
        }

        var normalizedTicketTypeCode = request.TicketTypeCode.Trim().ToUpperInvariant();
        var exists = await dbContext.ParkTicketTypes.AnyAsync(
            x => x.ParkId == request.ParkId && x.TicketTypeCode.ToUpper() == normalizedTicketTypeCode,
            cancellationToken);

        if (exists)
        {
            return Conflict(ApiResponse<ParkTicketTypeDto>.Fail("Mã loại vé đã tồn tại trong khu vui chơi này."));
        }

        var nowUtc = DateTime.UtcNow;
        var ticketType = new ParkTicketType
        {
            ParkId = request.ParkId,
            Code = request.Code.Trim(),
            TicketTypeCode = request.TicketTypeCode.Trim(),
            Name = request.Name.Trim(),
            TicketGroupName = TrimOrNull(request.TicketGroupName),
            CostPrice = request.CostPrice,
            Status = request.Status,
            CreatedAtUtc = nowUtc,
            CreatedByUserId = CurrentUserId,
            IsDeleted = false,
        };

        dbContext.ParkTicketTypes.Add(ticketType);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = ToDto(ticketType, park);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Park",
            EntityName = "ParkTicketType",
            EntityId = ticketType.Id.ToString(),
            Action = AuditAction.Create,
            After = dto,
        }, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = ticketType.Id }, ApiResponse<ParkTicketTypeDto>.Ok(dto, "Tạo loại vé thành công."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ParkTicketTypeDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var dto = await (
            from ticketType in dbContext.ParkTicketTypes.AsNoTracking()
            join park in dbContext.Parks.AsNoTracking() on ticketType.ParkId equals park.Id
            where ticketType.Id == id && !ticketType.IsDeleted && !park.IsDeleted
            select new ParkTicketTypeDto
            {
                Id = ticketType.Id,
                ParkId = ticketType.ParkId,
                ParkCode = park.Code,
                ParkName = park.Name,
                PaymentType = park.PaymentType,
                Code = ticketType.Code,
                TicketTypeCode = ticketType.TicketTypeCode,
                Name = ticketType.Name,
                TicketGroupName = ticketType.TicketGroupName,
                CostPrice = ticketType.CostPrice,
                Status = ticketType.Status,
                IsDeleted = ticketType.IsDeleted,
                CreatedAtUtc = ticketType.CreatedAtUtc,
                UpdatedAtUtc = ticketType.UpdatedAtUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
        {
            return NotFound(ApiResponse<ParkTicketTypeDto>.Fail("Không tìm thấy loại vé."));
        }

        return Ok(ApiResponse<ParkTicketTypeDto>.Ok(dto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ParkTicketTypeDto>>> Update(
        int id,
        UpdateParkTicketTypeRequest request,
        CancellationToken cancellationToken)
    {
        if (request.CostPrice < 0)
        {
            return BadRequest(ApiResponse<ParkTicketTypeDto>.Fail("Giá vốn không được âm."));
        }

        var ticketType = await dbContext.ParkTicketTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (ticketType is null)
        {
            return NotFound(ApiResponse<ParkTicketTypeDto>.Fail("Không tìm thấy loại vé."));
        }

        var park = await dbContext.Parks.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ParkId && !x.IsDeleted, cancellationToken);
        if (park is null)
        {
            return BadRequest(ApiResponse<ParkTicketTypeDto>.Fail("Khu vui chơi không tồn tại."));
        }

        var normalizedTicketTypeCode = request.TicketTypeCode.Trim().ToUpperInvariant();
        var exists = await dbContext.ParkTicketTypes.AnyAsync(
            x => x.Id != id && x.ParkId == request.ParkId && x.TicketTypeCode.ToUpper() == normalizedTicketTypeCode,
            cancellationToken);

        if (exists)
        {
            return Conflict(ApiResponse<ParkTicketTypeDto>.Fail("Mã loại vé đã tồn tại trong khu vui chơi này."));
        }

        var oldPark = await dbContext.Parks.AsNoTracking().FirstAsync(x => x.Id == ticketType.ParkId, cancellationToken);
        var before = ToDto(ticketType, oldPark);

        ticketType.ParkId = request.ParkId;
        ticketType.Code = request.Code.Trim();
        ticketType.TicketTypeCode = request.TicketTypeCode.Trim();
        ticketType.Name = request.Name.Trim();
        ticketType.TicketGroupName = TrimOrNull(request.TicketGroupName);
        ticketType.CostPrice = request.CostPrice;
        ticketType.Status = request.Status;
        ticketType.UpdatedAtUtc = DateTime.UtcNow;
        ticketType.UpdatedByUserId = CurrentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);

        var after = ToDto(ticketType, park);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Park",
            EntityName = "ParkTicketType",
            EntityId = ticketType.Id.ToString(),
            Action = AuditAction.Update,
            Before = before,
            After = after,
        }, cancellationToken);

        return Ok(ApiResponse<ParkTicketTypeDto>.Ok(after, "Cập nhật loại vé thành công."));
    }

    [HttpPost("{id:int}/set-inactive")]
    public async Task<ActionResult<ApiResponse<ParkTicketTypeDto>>> SetInactive(int id, CancellationToken cancellationToken)
    {
        var ticketType = await dbContext.ParkTicketTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (ticketType is null)
        {
            return NotFound(ApiResponse<ParkTicketTypeDto>.Fail("Không tìm thấy loại vé."));
        }

        var park = await dbContext.Parks.AsNoTracking().FirstAsync(x => x.Id == ticketType.ParkId, cancellationToken);
        var before = ToDto(ticketType, park);

        ticketType.Status = RecordStatus.Inactive;
        ticketType.UpdatedAtUtc = DateTime.UtcNow;
        ticketType.UpdatedByUserId = CurrentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);

        var after = ToDto(ticketType, park);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Park",
            EntityName = "ParkTicketType",
            EntityId = ticketType.Id.ToString(),
            Action = AuditAction.SetInactive,
            Before = before,
            After = after,
        }, cancellationToken);

        return Ok(ApiResponse<ParkTicketTypeDto>.Ok(after, "Đã ngừng sử dụng loại vé."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(int id, CancellationToken cancellationToken)
    {
        if (!User.IsInRole(nameof(UserRole.Admin)))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object?>.Fail("Chỉ Admin được xóa mềm loại vé."));
        }

        var ticketType = await dbContext.ParkTicketTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (ticketType is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy loại vé."));
        }

        var park = await dbContext.Parks.AsNoTracking().FirstAsync(x => x.Id == ticketType.ParkId, cancellationToken);
        var before = ToDto(ticketType, park);

        ticketType.IsDeleted = true;
        ticketType.Status = RecordStatus.Inactive;
        ticketType.DeletedAtUtc = DateTime.UtcNow;
        ticketType.DeletedByUserId = CurrentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Park",
            EntityName = "ParkTicketType",
            EntityId = ticketType.Id.ToString(),
            Action = AuditAction.SoftDelete,
            Before = before,
            After = ToDto(ticketType, park),
        }, cancellationToken);

        return Ok(ApiResponse<object?>.Ok(null, "Xóa mềm loại vé thành công."));
    }

    [HttpGet("{id:int}/audit-logs")]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLogDto>>>> GetAuditLogs(
        int id,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var entityId = id.ToString();
        var query = dbContext.AuditLogs.AsNoTracking()
            .Where(x => x.Module == "Park" && x.EntityName == "ParkTicketType" && x.EntityId == entityId);

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

        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(new PagedResult<AuditLogDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<AuditLogDto>.FixedPageSize),
        }));
    }

    private static string? TrimOrNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static ParkTicketTypeDto ToDto(ParkTicketType ticketType, Park park)
    {
        return new ParkTicketTypeDto
        {
            Id = ticketType.Id,
            ParkId = ticketType.ParkId,
            ParkCode = park.Code,
            ParkName = park.Name,
            PaymentType = park.PaymentType,
            Code = ticketType.Code,
            TicketTypeCode = ticketType.TicketTypeCode,
            Name = ticketType.Name,
            TicketGroupName = ticketType.TicketGroupName,
            CostPrice = ticketType.CostPrice,
            Status = ticketType.Status,
            IsDeleted = ticketType.IsDeleted,
            CreatedAtUtc = ticketType.CreatedAtUtc,
            UpdatedAtUtc = ticketType.UpdatedAtUtc,
        };
    }
}
