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
[Route("api/parks")]
public sealed class ParksController(
    PpvReconDbContext dbContext,
    IAuditService auditService) : PpvControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ParkDto>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] string? keyword = null,
        [FromQuery] ParkPaymentType? paymentType = null,
        [FromQuery] RecordStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query = dbContext.Parks.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var trimmed = keyword.Trim();
            var normalized = trimmed.ToUpperInvariant();
            query = query.Where(x =>
                x.Code.ToUpper().Contains(normalized)
                || x.Name.ToUpper().Contains(normalized)
                || (x.SearchCode != null && x.SearchCode.ToUpper().Contains(normalized))
                || (x.BankAccount != null && x.BankAccount.Contains(trimmed)));
        }

        if (paymentType is not null)
        {
            query = query.Where(x => x.PaymentType == paymentType);
        }

        if (status is not null)
        {
            query = query.Where(x => x.Status == status);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * PagedResult<ParkDto>.FixedPageSize)
            .Take(PagedResult<ParkDto>.FixedPageSize)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<ParkDto>>.Ok(new PagedResult<ParkDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<ParkDto>.FixedPageSize),
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ParkDto>>> Create(CreateParkRequest request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var codeExists = await dbContext.Parks.AnyAsync(x => x.Code.ToUpper() == normalizedCode, cancellationToken);
        if (codeExists)
        {
            return Conflict(ApiResponse<ParkDto>.Fail("Mã KVC đã tồn tại."));
        }

        var nowUtc = DateTime.UtcNow;
        var park = new Park
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            PaymentType = request.PaymentType,
            SearchCode = TrimOrNull(request.SearchCode),
            Location = TrimOrNull(request.Location),
            BankAccount = TrimOrNull(request.BankAccount),
            BankName = TrimOrNull(request.BankName),
            CreditLimit = request.CreditLimit,
            ApiSiteId = TrimOrNull(request.ApiSiteId),
            ApiProfileId = TrimOrNull(request.ApiProfileId),
            BalanceTransformRule = TrimOrNull(request.BalanceTransformRule),
            ApiNote = TrimOrNull(request.ApiNote),
            Status = request.Status,
            CreatedAtUtc = nowUtc,
            CreatedByUserId = CurrentUserId,
            IsDeleted = false,
        };

        dbContext.Parks.Add(park);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Park",
            EntityName = "Park",
            EntityId = park.Id.ToString(),
            Action = AuditAction.Create,
            After = ToDto(park),
        }, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = park.Id }, ApiResponse<ParkDto>.Ok(ToDto(park), "Tạo khu vui chơi thành công."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ParkDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var park = await dbContext.Parks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (park is null)
        {
            return NotFound(ApiResponse<ParkDto>.Fail("Không tìm thấy khu vui chơi."));
        }

        return Ok(ApiResponse<ParkDto>.Ok(ToDto(park)));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ParkDto>>> Update(
        int id,
        UpdateParkRequest request,
        CancellationToken cancellationToken)
    {
        var park = await dbContext.Parks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (park is null)
        {
            return NotFound(ApiResponse<ParkDto>.Fail("Không tìm thấy khu vui chơi."));
        }

        var before = ToDto(park);
        park.Name = request.Name.Trim();
        park.PaymentType = request.PaymentType;
        park.SearchCode = TrimOrNull(request.SearchCode);
        park.Location = TrimOrNull(request.Location);
        park.BankAccount = TrimOrNull(request.BankAccount);
        park.BankName = TrimOrNull(request.BankName);
        park.CreditLimit = request.CreditLimit;
        park.ApiSiteId = TrimOrNull(request.ApiSiteId);
        park.ApiProfileId = TrimOrNull(request.ApiProfileId);
        park.BalanceTransformRule = TrimOrNull(request.BalanceTransformRule);
        park.ApiNote = TrimOrNull(request.ApiNote);
        park.Status = request.Status;
        park.UpdatedAtUtc = DateTime.UtcNow;
        park.UpdatedByUserId = CurrentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Park",
            EntityName = "Park",
            EntityId = park.Id.ToString(),
            Action = AuditAction.Update,
            Before = before,
            After = ToDto(park),
        }, cancellationToken);

        return Ok(ApiResponse<ParkDto>.Ok(ToDto(park), "Cập nhật khu vui chơi thành công."));
    }

    [HttpPost("{id:int}/set-inactive")]
    public async Task<ActionResult<ApiResponse<ParkDto>>> SetInactive(int id, CancellationToken cancellationToken)
    {
        var park = await dbContext.Parks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (park is null)
        {
            return NotFound(ApiResponse<ParkDto>.Fail("Không tìm thấy khu vui chơi."));
        }

        var before = ToDto(park);
        park.Status = RecordStatus.Inactive;
        park.UpdatedAtUtc = DateTime.UtcNow;
        park.UpdatedByUserId = CurrentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Park",
            EntityName = "Park",
            EntityId = park.Id.ToString(),
            Action = AuditAction.SetInactive,
            Before = before,
            After = ToDto(park),
        }, cancellationToken);

        return Ok(ApiResponse<ParkDto>.Ok(ToDto(park), "Đã ngừng sử dụng khu vui chơi."));
    }

    [HttpPost("{id:int}/restore")]
    public async Task<ActionResult<ApiResponse<ParkDto>>> Restore(int id, CancellationToken cancellationToken)
    {
        if (!User.IsInRole(nameof(UserRole.Admin)))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<ParkDto>.Fail("Chỉ Admin được khôi phục dữ liệu đã xóa."));
        }

        var park = await dbContext.Parks.FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted, cancellationToken);
        if (park is null)
        {
            return NotFound(ApiResponse<ParkDto>.Fail("Không tìm thấy khu vui chơi đã xóa."));
        }

        var before = ToDto(park);
        park.IsDeleted = false;
        park.DeletedAtUtc = null;
        park.DeletedByUserId = null;
        park.UpdatedAtUtc = DateTime.UtcNow;
        park.UpdatedByUserId = CurrentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Park",
            EntityName = "Park",
            EntityId = park.Id.ToString(),
            Action = AuditAction.Restore,
            Before = before,
            After = ToDto(park),
        }, cancellationToken);

        return Ok(ApiResponse<ParkDto>.Ok(ToDto(park), "Khôi phục khu vui chơi thành công."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(int id, CancellationToken cancellationToken)
    {
        if (!User.IsInRole(nameof(UserRole.Admin)))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object?>.Fail("Chỉ Admin được xóa mềm khu vui chơi."));
        }

        var park = await dbContext.Parks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (park is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy khu vui chơi."));
        }

        var before = ToDto(park);
        park.IsDeleted = true;
        park.DeletedAtUtc = DateTime.UtcNow;
        park.DeletedByUserId = CurrentUserId;
        park.Status = RecordStatus.Inactive;

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Park",
            EntityName = "Park",
            EntityId = park.Id.ToString(),
            Action = AuditAction.SoftDelete,
            Before = before,
            After = ToDto(park),
        }, cancellationToken);

        return Ok(ApiResponse<object?>.Ok(null, "Xóa mềm khu vui chơi thành công."));
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
            .Where(x => x.Module == "Park" && x.EntityName == "Park" && x.EntityId == entityId);

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

    private static ParkDto ToDto(Park park)
    {
        return new ParkDto
        {
            Id = park.Id,
            Code = park.Code,
            Name = park.Name,
            PaymentType = park.PaymentType,
            SearchCode = park.SearchCode,
            Location = park.Location,
            BankAccount = park.BankAccount,
            BankName = park.BankName,
            CreditLimit = park.CreditLimit,
            ApiSiteId = park.ApiSiteId,
            ApiProfileId = park.ApiProfileId,
            BalanceTransformRule = park.BalanceTransformRule,
            ApiNote = park.ApiNote,
            Status = park.Status,
            IsDeleted = park.IsDeleted,
            CreatedAtUtc = park.CreatedAtUtc,
            UpdatedAtUtc = park.UpdatedAtUtc,
        };
    }
}
