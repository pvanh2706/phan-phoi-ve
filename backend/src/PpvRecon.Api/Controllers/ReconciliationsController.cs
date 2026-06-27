using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Common;
using PpvRecon.Application.Reconciliation;
using PpvRecon.Domain.Entities.Reconciliation;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reconciliations")]
public sealed class ReconciliationsController(
    PpvReconDbContext dbContext,
    IReconciliationBuilder reconciliationBuilder,
    IAuditService auditService) : PpvControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ParkReconciliationDto>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] DateOnly? businessDate = null,
        [FromQuery] int? parkId = null,
        [FromQuery] ParkPaymentType? paymentType = null,
        [FromQuery] ReconciliationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query =
            from reconciliation in dbContext.ParkReconciliations.AsNoTracking()
            join park in dbContext.Parks.AsNoTracking() on reconciliation.ParkId equals park.Id
            where !park.IsDeleted
            select new { reconciliation, park };

        if (businessDate is not null) query = query.Where(x => x.reconciliation.BusinessDate == businessDate);
        if (parkId is not null) query = query.Where(x => x.reconciliation.ParkId == parkId);
        if (paymentType is not null) query = query.Where(x => x.reconciliation.PaymentType == paymentType);
        if (status is not null) query = query.Where(x => x.reconciliation.Status == status);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.reconciliation.BusinessDate)
            .ThenBy(x => x.park.Name)
            .Skip((page - 1) * PagedResult<ParkReconciliationDto>.FixedPageSize)
            .Take(PagedResult<ParkReconciliationDto>.FixedPageSize)
            .Select(x => ToDto(x.reconciliation, x.park.Code, x.park.Name))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<ParkReconciliationDto>>.Ok(new PagedResult<ParkReconciliationDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<ParkReconciliationDto>.FixedPageSize),
        }));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ParkReconciliationDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var dto = await (
            from reconciliation in dbContext.ParkReconciliations.AsNoTracking()
            join park in dbContext.Parks.AsNoTracking() on reconciliation.ParkId equals park.Id
            where reconciliation.Id == id && !park.IsDeleted
            select ToDto(reconciliation, park.Code, park.Name))
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
        {
            return NotFound(ApiResponse<ParkReconciliationDto>.Fail("Không tìm thấy dòng đối soát."));
        }

        return Ok(ApiResponse<ParkReconciliationDto>.Ok(dto));
    }

    [HttpPost("build")]
    public async Task<ActionResult<ApiResponse<BuildReconciliationResultDto>>> Build(
        BuildReconciliationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await reconciliationBuilder.BuildAsync(request.BusinessDate, CurrentUserId, cancellationToken);
        return Ok(ApiResponse<BuildReconciliationResultDto>.Ok(result, "Đã build đối soát khu vui chơi."));
    }

    [Authorize(Roles = "Admin,Accountant")]
    [HttpPost("{id:int}/resolve")]
    public async Task<ActionResult<ApiResponse<ParkReconciliationDto>>> Resolve(
        int id,
        ResolveReconciliationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AdjustmentNote))
        {
            return BadRequest(ApiResponse<ParkReconciliationDto>.Fail("Ghi chú xử lý là bắt buộc."));
        }

        var reconciliation = await dbContext.ParkReconciliations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (reconciliation is null)
        {
            return NotFound(ApiResponse<ParkReconciliationDto>.Fail("Không tìm thấy dòng đối soát."));
        }

        var park = await dbContext.Parks.AsNoTracking()
            .FirstAsync(x => x.Id == reconciliation.ParkId, cancellationToken);
        var before = ToDto(reconciliation, park.Code, park.Name);

        reconciliation.AdjustmentAmount = request.AdjustmentAmount;
        reconciliation.AdjustmentNote = request.AdjustmentNote.Trim();
        reconciliation.Status = ReconciliationStatus.Resolved;
        reconciliation.ResolvedByUserId = CurrentUserId;
        reconciliation.ResolvedAtUtc = DateTime.UtcNow;
        reconciliation.ResolvedSourceHash = reconciliation.LastSourceHash;
        reconciliation.SourceChangedAfterResolved = false;
        reconciliation.UpdatedAtUtc = DateTime.UtcNow;
        reconciliation.UpdatedByUserId = CurrentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);

        var after = ToDto(reconciliation, park.Code, park.Name);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Park",
            EntityName = "ParkReconciliation",
            EntityId = reconciliation.Id.ToString(),
            Action = AuditAction.ResolveVariance,
            Before = before,
            After = after,
        }, cancellationToken);

        return Ok(ApiResponse<ParkReconciliationDto>.Ok(after, "Đã xử lý dòng đối soát."));
    }

    private static ParkReconciliationDto ToDto(ParkReconciliation reconciliation, string parkCode, string parkName)
    {
        return new ParkReconciliationDto
        {
            Id = reconciliation.Id,
            BusinessDate = reconciliation.BusinessDate,
            PreviousBusinessDate = reconciliation.PreviousBusinessDate,
            ParkId = reconciliation.ParkId,
            ParkCode = parkCode,
            ParkName = parkName,
            PaymentType = reconciliation.PaymentType,
            PreviousBalance = reconciliation.PreviousBalance,
            AdditionalAmount = reconciliation.AdditionalAmount,
            UsedAmount = reconciliation.UsedAmount,
            ExpectedBalance = reconciliation.ExpectedBalance,
            ActualBalance = reconciliation.ActualBalance,
            VarianceAmount = reconciliation.VarianceAmount,
            AdjustmentAmount = reconciliation.AdjustmentAmount,
            AdjustmentNote = reconciliation.AdjustmentNote,
            Status = reconciliation.Status,
            MissingPreviousBalance = reconciliation.MissingPreviousBalance,
            MissingActualBalance = reconciliation.MissingActualBalance,
            MissingTicketCost = reconciliation.MissingTicketCost,
            MissingBankTransaction = reconciliation.MissingBankTransaction,
            ResolvedByUserId = reconciliation.ResolvedByUserId,
            ResolvedAtUtc = reconciliation.ResolvedAtUtc,
            LastBuiltJobRunId = reconciliation.LastBuiltJobRunId,
            RebuildCount = reconciliation.RebuildCount,
            LastSourceHash = reconciliation.LastSourceHash,
            ResolvedSourceHash = reconciliation.ResolvedSourceHash,
            SourceChangedAfterResolved = reconciliation.SourceChangedAfterResolved,
            CreatedAtUtc = reconciliation.CreatedAtUtc,
            UpdatedAtUtc = reconciliation.UpdatedAtUtc,
        };
    }
}
