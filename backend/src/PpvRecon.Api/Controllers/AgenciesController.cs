using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Agencies;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Common;
using PpvRecon.Domain.Entities.Agencies;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/agencies")]
public sealed class AgenciesController(
    PpvReconDbContext dbContext,
    IAuditService auditService) : PpvControllerBase
{
    private const int PageSize = 20;
    private static readonly HashSet<string> AllowedSources = new(StringComparer.OrdinalIgnoreCase)
    {
        "OneInventory",
        "AR",
        "AR & OneInventory",
    };

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AgencyDto>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query = dbContext.Agencies.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalized = keyword.Trim().ToUpperInvariant();
            query = query.Where(x =>
                x.Code.ToUpper().Contains(normalized)
                || x.Name.ToUpper().Contains(normalized));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<AgencyDto>>.Ok(new PagedResult<AgencyDto>
        {
            Items = items,
            Page = page,
            PageSize = PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize),
        }));
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<ApiResponse<AgencyDto>>> Create(
        CreateAgencyRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponse<AgencyDto>.Fail(validationError));
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var codeExists = await dbContext.Agencies
            .AnyAsync(x => x.Code.ToUpper() == normalizedCode, cancellationToken);
        if (codeExists)
        {
            return Conflict(ApiResponse<AgencyDto>.Fail("Mã đại lý đã tồn tại."));
        }

        var agency = new Agency
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            ParentCode = TrimOrNull(request.ParentCode),
            ParentName = TrimOrNull(request.ParentName),
            Source = NormalizeSource(request.Source),
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = CurrentUserId,
        };

        dbContext.Agencies.Add(agency);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Agency",
            EntityName = "Agency",
            EntityId = agency.Id.ToString(),
            Action = AuditAction.Create,
            After = ToDto(agency),
        }, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = agency.Id },
            ApiResponse<AgencyDto>.Ok(ToDto(agency), "Tạo đại lý thành công."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<AgencyDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var agency = await dbContext.Agencies.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (agency is null)
        {
            return NotFound(ApiResponse<AgencyDto>.Fail("Không tìm thấy đại lý."));
        }

        return Ok(ApiResponse<AgencyDto>.Ok(ToDto(agency)));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<ApiResponse<AgencyDto>>> Update(
        int id,
        UpdateAgencyRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponse<AgencyDto>.Fail(validationError));
        }

        var agency = await dbContext.Agencies
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (agency is null)
        {
            return NotFound(ApiResponse<AgencyDto>.Fail("Không tìm thấy đại lý."));
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var codeExists = await dbContext.Agencies
            .AnyAsync(x => x.Id != id && x.Code.ToUpper() == normalizedCode, cancellationToken);
        if (codeExists)
        {
            return Conflict(ApiResponse<AgencyDto>.Fail("Mã đại lý đã tồn tại."));
        }

        var before = ToDto(agency);
        agency.Code = request.Code.Trim();
        agency.Name = request.Name.Trim();
        agency.ParentCode = TrimOrNull(request.ParentCode);
        agency.ParentName = TrimOrNull(request.ParentName);
        agency.Source = NormalizeSource(request.Source);
        agency.UpdatedAtUtc = DateTime.UtcNow;
        agency.UpdatedByUserId = CurrentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Agency",
            EntityName = "Agency",
            EntityId = agency.Id.ToString(),
            Action = AuditAction.Update,
            Before = before,
            After = ToDto(agency),
        }, cancellationToken);

        return Ok(ApiResponse<AgencyDto>.Ok(ToDto(agency), "Cập nhật đại lý thành công."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var agency = await dbContext.Agencies
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (agency is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy đại lý."));
        }

        var before = ToDto(agency);
        agency.IsDeleted = true;
        agency.DeletedAtUtc = DateTime.UtcNow;
        agency.DeletedByUserId = CurrentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Agency",
            EntityName = "Agency",
            EntityId = agency.Id.ToString(),
            Action = AuditAction.SoftDelete,
            Before = before,
            After = ToDto(agency),
        }, cancellationToken);

        return Ok(ApiResponse<object?>.Ok(null, "Xóa đại lý thành công."));
    }

    private static string? ValidateRequest(SaveAgencyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return "Mã đại lý là bắt buộc.";
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return "Tên đại lý là bắt buộc.";
        }

        return AllowedSources.Contains(request.Source.Trim())
            ? null
            : "Nguồn dữ liệu phải là OneInventory, AR hoặc AR & OneInventory.";
    }

    private static string NormalizeSource(string source)
    {
        var trimmed = source.Trim();
        return AllowedSources.First(x => x.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
    }

    private static string? TrimOrNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static AgencyDto ToDto(Agency agency)
    {
        return new AgencyDto
        {
            Id = agency.Id,
            Code = agency.Code,
            Name = agency.Name,
            ParentCode = agency.ParentCode,
            ParentName = agency.ParentName,
            Source = agency.Source,
            CreatedAtUtc = agency.CreatedAtUtc,
            UpdatedAtUtc = agency.UpdatedAtUtc,
        };
    }
}
