using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Common;
using PpvRecon.Application.Summaries;
using PpvRecon.Domain.Entities.Jobs;
using PpvRecon.Domain.Entities.Parks;
using PpvRecon.Domain.Entities.Summaries;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/park-balances")]
public sealed class ParkBalancesController(PpvReconDbContext dbContext) : PpvControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<DailyParkBalanceSnapshotDto>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] DateOnly? businessDate = null,
        [FromQuery] int? parkId = null,
        [FromQuery] ParkPaymentType? paymentType = null,
        [FromQuery] SourceType? sourceType = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query =
            from summary in dbContext.DailyParkBalanceSnapshots.AsNoTracking()
            join park in dbContext.Parks.AsNoTracking() on summary.ParkId equals park.Id
            where !park.IsDeleted
            select new { summary, park };

        if (businessDate is not null)
        {
            query = query.Where(x => x.summary.BusinessDate == businessDate);
        }

        if (parkId is not null)
        {
            query = query.Where(x => x.summary.ParkId == parkId);
        }

        if (paymentType is not null)
        {
            query = query.Where(x => x.summary.PaymentType == paymentType);
        }

        if (sourceType is not null)
        {
            query = query.Where(x => x.summary.SourceType == sourceType);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.summary.BusinessDate)
            .ThenBy(x => x.park.Name)
            .Skip((page - 1) * PagedResult<DailyParkBalanceSnapshotDto>.FixedPageSize)
            .Take(PagedResult<DailyParkBalanceSnapshotDto>.FixedPageSize)
            .Select(x => new DailyParkBalanceSnapshotDto
            {
                Id = x.summary.Id,
                BusinessDate = x.summary.BusinessDate,
                ParkId = x.summary.ParkId,
                ParkCode = x.park.Code,
                ParkName = x.park.Name,
                PaymentType = x.summary.PaymentType,
                AvailableBalance = x.summary.AvailableBalance,
                BankAccountSnapshot = x.summary.BankAccountSnapshot,
                SourceType = x.summary.SourceType,
                SourceJobRunId = x.summary.SourceJobRunId,
                SourceJobRunItemId = x.summary.SourceJobRunItemId,
                RawResponseId = x.summary.RawResponseId,
                CreatedAtUtc = x.summary.CreatedAtUtc,
                UpdatedAtUtc = x.summary.UpdatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<DailyParkBalanceSnapshotDto>>.Ok(new PagedResult<DailyParkBalanceSnapshotDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<DailyParkBalanceSnapshotDto>.FixedPageSize),
        }));
    }
}

[ApiController]
[Authorize]
[Route("api/ticket-cost-summaries")]
public sealed class TicketCostSummariesController(
    PpvReconDbContext dbContext,
    IAuditService auditService) : PpvControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<DailyTicketCostSummaryDto>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] DateOnly? businessDate = null,
        [FromQuery] int? parkId = null,
        [FromQuery] ParkPaymentType? paymentType = null,
        [FromQuery] SourceType? sourceType = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query =
            from summary in dbContext.DailyTicketCostSummaries.AsNoTracking()
            join park in dbContext.Parks.AsNoTracking() on summary.ParkId equals park.Id
            where !park.IsDeleted
            select new { summary, park };

        if (businessDate is not null) query = query.Where(x => x.summary.BusinessDate == businessDate);
        if (parkId is not null) query = query.Where(x => x.summary.ParkId == parkId);
        if (paymentType is not null) query = query.Where(x => x.summary.PaymentType == paymentType);
        if (sourceType is not null) query = query.Where(x => x.summary.SourceType == sourceType);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.summary.BusinessDate)
            .ThenBy(x => x.park.Name)
            .Skip((page - 1) * PagedResult<DailyTicketCostSummaryDto>.FixedPageSize)
            .Take(PagedResult<DailyTicketCostSummaryDto>.FixedPageSize)
            .Select(x => new DailyTicketCostSummaryDto
            {
                Id = x.summary.Id,
                BusinessDate = x.summary.BusinessDate,
                ParkId = x.summary.ParkId,
                ParkCode = x.park.Code,
                ParkName = x.park.Name,
                PaymentType = x.summary.PaymentType,
                TotalTicketCost = x.summary.TotalTicketCost,
                TotalSalesAmount = x.summary.TotalSalesAmount,
                TotalQuantity = x.summary.TotalQuantity,
                SourceType = x.summary.SourceType,
                SourceJobRunId = x.summary.SourceJobRunId,
                SourceJobRunItemId = x.summary.SourceJobRunItemId,
                RawResponseId = x.summary.RawResponseId,
                ManualReason = x.summary.ManualReason,
                CreatedAtUtc = x.summary.CreatedAtUtc,
                UpdatedAtUtc = x.summary.UpdatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<DailyTicketCostSummaryDto>>.Ok(new PagedResult<DailyTicketCostSummaryDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<DailyTicketCostSummaryDto>.FixedPageSize),
        }));
    }

    [Authorize(Roles = "Admin,Accountant")]
    [HttpPost("manual")]
    public async Task<ActionResult<ApiResponse<DailyTicketCostSummaryDto>>> Manual(
        ManualTicketCostSummaryRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TotalTicketCost < 0 || request.TotalSalesAmount < 0 || request.TotalQuantity < 0)
        {
            return BadRequest(ApiResponse<DailyTicketCostSummaryDto>.Fail("Số lượng và số tiền không được âm."));
        }

        if (string.IsNullOrWhiteSpace(request.ManualReason))
        {
            return BadRequest(ApiResponse<DailyTicketCostSummaryDto>.Fail("Lý do nhập tay là bắt buộc."));
        }

        var park = await dbContext.Parks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ParkId && !x.IsDeleted, cancellationToken);
        if (park is null)
        {
            return BadRequest(ApiResponse<DailyTicketCostSummaryDto>.Fail("Khu vui chơi không tồn tại."));
        }

        var jobInfo = await ResolveJobItemAsync(request.JobRunItemId, ExternalApiSource.TicketCost, cancellationToken);
        if (jobInfo.ErrorMessage is not null)
        {
            return BadRequest(ApiResponse<DailyTicketCostSummaryDto>.Fail(jobInfo.ErrorMessage));
        }

        var nowUtc = DateTime.UtcNow;
        var existing = await dbContext.DailyTicketCostSummaries
            .FirstOrDefaultAsync(x => x.BusinessDate == request.BusinessDate && x.ParkId == request.ParkId, cancellationToken);
        var before = existing is null ? null : ToDto(existing, park);

        if (existing is null)
        {
            existing = new DailyTicketCostSummary
            {
                BusinessDate = request.BusinessDate,
                ParkId = request.ParkId,
                CreatedAtUtc = nowUtc,
                CreatedByUserId = CurrentUserId,
            };
            dbContext.DailyTicketCostSummaries.Add(existing);
        }
        else
        {
            existing.UpdatedAtUtc = nowUtc;
            existing.UpdatedByUserId = CurrentUserId;
        }

        existing.PaymentType = park.PaymentType;
        existing.TotalTicketCost = request.TotalTicketCost;
        existing.TotalSalesAmount = request.TotalSalesAmount;
        existing.TotalQuantity = request.TotalQuantity;
        existing.SourceType = SourceType.Manual;
        existing.SourceJobRunId = jobInfo.JobRunId;
        existing.SourceJobRunItemId = request.JobRunItemId;
        existing.RawResponseId = jobInfo.RawResponseId;
        existing.ManualReason = request.ManualReason.Trim();

        MarkJobItemResolved(jobInfo.JobRunItem, nowUtc, request.ManualReason);
        await dbContext.SaveChangesAsync(cancellationToken);

        var after = ToDto(existing, park);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Park",
            EntityName = "DailyTicketCostSummary",
            EntityId = existing.Id.ToString(),
            Action = AuditAction.ManualEntry,
            Before = before,
            After = after,
        }, cancellationToken);

        return Ok(ApiResponse<DailyTicketCostSummaryDto>.Ok(after, "Nhập tay tổng giá vốn thành công."));
    }

    private async Task<JobItemResolution> ResolveJobItemAsync(int? jobRunItemId, ExternalApiSource expectedSource, CancellationToken cancellationToken)
    {
        if (jobRunItemId is null) return new JobItemResolution();
        var item = await dbContext.JobRunItems.FirstOrDefaultAsync(x => x.Id == jobRunItemId, cancellationToken);
        if (item is null) return new JobItemResolution { ErrorMessage = "Không tìm thấy lỗi job cần xử lý." };
        if (item.Source != expectedSource) return new JobItemResolution { ErrorMessage = "Loại lỗi job không khớp với dữ liệu nhập tay." };
        return new JobItemResolution { JobRunId = item.JobRunId, RawResponseId = item.RawResponseId, JobRunItem = item };
    }

    private void MarkJobItemResolved(JobRunItem? item, DateTime nowUtc, string note)
    {
        if (item is null || item.Status != JobRunItemStatus.Failed) return;
        item.Status = JobRunItemStatus.ManualResolved;
        item.ResolvedByUserId = CurrentUserId;
        item.ResolvedAtUtc = nowUtc;
        item.ManualResolutionNote = note.Trim();
    }

    private static DailyTicketCostSummaryDto ToDto(DailyTicketCostSummary summary, Park park)
    {
        return new DailyTicketCostSummaryDto
        {
            Id = summary.Id,
            BusinessDate = summary.BusinessDate,
            ParkId = summary.ParkId,
            ParkCode = park.Code,
            ParkName = park.Name,
            PaymentType = summary.PaymentType,
            TotalTicketCost = summary.TotalTicketCost,
            TotalSalesAmount = summary.TotalSalesAmount,
            TotalQuantity = summary.TotalQuantity,
            SourceType = summary.SourceType,
            SourceJobRunId = summary.SourceJobRunId,
            SourceJobRunItemId = summary.SourceJobRunItemId,
            RawResponseId = summary.RawResponseId,
            ManualReason = summary.ManualReason,
            CreatedAtUtc = summary.CreatedAtUtc,
            UpdatedAtUtc = summary.UpdatedAtUtc,
        };
    }
}

[ApiController]
[Authorize]
[Route("api/bank-transaction-summaries")]
public sealed class BankTransactionSummariesController(
    PpvReconDbContext dbContext,
    IAuditService auditService) : PpvControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<DailyBankTransactionSummaryDto>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] DateOnly? businessDate = null,
        [FromQuery] int? parkId = null,
        [FromQuery] ParkPaymentType? paymentType = null,
        [FromQuery] SourceType? sourceType = null,
        [FromQuery] BankTransactionType? transactionType = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query =
            from summary in dbContext.DailyBankTransactionSummaries.AsNoTracking()
            join park in dbContext.Parks.AsNoTracking() on summary.ParkId equals park.Id
            where !park.IsDeleted
            select new { summary, park };

        if (businessDate is not null) query = query.Where(x => x.summary.BusinessDate == businessDate);
        if (parkId is not null) query = query.Where(x => x.summary.ParkId == parkId);
        if (paymentType is not null) query = query.Where(x => x.summary.PaymentType == paymentType);
        if (sourceType is not null) query = query.Where(x => x.summary.SourceType == sourceType);
        if (transactionType is not null) query = query.Where(x => x.summary.TransactionType == transactionType);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.summary.BusinessDate)
            .ThenBy(x => x.park.Name)
            .ThenBy(x => x.summary.TransactionType)
            .Skip((page - 1) * PagedResult<DailyBankTransactionSummaryDto>.FixedPageSize)
            .Take(PagedResult<DailyBankTransactionSummaryDto>.FixedPageSize)
            .Select(x => new DailyBankTransactionSummaryDto
            {
                Id = x.summary.Id,
                BusinessDate = x.summary.BusinessDate,
                ParkId = x.summary.ParkId,
                ParkCode = x.park.Code,
                ParkName = x.park.Name,
                PaymentType = x.summary.PaymentType,
                TransactionType = x.summary.TransactionType,
                TotalDebitAmount = x.summary.TotalDebitAmount,
                TotalCreditAmount = x.summary.TotalCreditAmount,
                TransactionCount = x.summary.TransactionCount,
                SourceType = x.summary.SourceType,
                SourceJobRunId = x.summary.SourceJobRunId,
                SourceJobRunItemId = x.summary.SourceJobRunItemId,
                RawResponseId = x.summary.RawResponseId,
                ManualReason = x.summary.ManualReason,
                CreatedAtUtc = x.summary.CreatedAtUtc,
                UpdatedAtUtc = x.summary.UpdatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<DailyBankTransactionSummaryDto>>.Ok(new PagedResult<DailyBankTransactionSummaryDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<DailyBankTransactionSummaryDto>.FixedPageSize),
        }));
    }

    [Authorize(Roles = "Admin,Accountant")]
    [HttpPost("manual")]
    public async Task<ActionResult<ApiResponse<DailyBankTransactionSummaryDto>>> Manual(
        ManualBankTransactionSummaryRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TotalDebitAmount < 0 || request.TotalCreditAmount < 0 || request.TransactionCount < 0)
        {
            return BadRequest(ApiResponse<DailyBankTransactionSummaryDto>.Fail("Số lượng và số tiền không được âm."));
        }

        if (string.IsNullOrWhiteSpace(request.ManualReason))
        {
            return BadRequest(ApiResponse<DailyBankTransactionSummaryDto>.Fail("Lý do nhập tay là bắt buộc."));
        }

        var park = await dbContext.Parks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ParkId && !x.IsDeleted, cancellationToken);
        if (park is null)
        {
            return BadRequest(ApiResponse<DailyBankTransactionSummaryDto>.Fail("Khu vui chơi không tồn tại."));
        }

        var jobInfo = await ResolveJobItemAsync(request.JobRunItemId, ExternalApiSource.BankTransaction, cancellationToken);
        if (jobInfo.ErrorMessage is not null)
        {
            return BadRequest(ApiResponse<DailyBankTransactionSummaryDto>.Fail(jobInfo.ErrorMessage));
        }

        var nowUtc = DateTime.UtcNow;
        var existing = await dbContext.DailyBankTransactionSummaries
            .FirstOrDefaultAsync(x => x.BusinessDate == request.BusinessDate
                && x.ParkId == request.ParkId
                && x.TransactionType == request.TransactionType, cancellationToken);
        var before = existing is null ? null : ToDto(existing, park);

        if (existing is null)
        {
            existing = new DailyBankTransactionSummary
            {
                BusinessDate = request.BusinessDate,
                ParkId = request.ParkId,
                TransactionType = request.TransactionType,
                CreatedAtUtc = nowUtc,
                CreatedByUserId = CurrentUserId,
            };
            dbContext.DailyBankTransactionSummaries.Add(existing);
        }
        else
        {
            existing.UpdatedAtUtc = nowUtc;
            existing.UpdatedByUserId = CurrentUserId;
        }

        existing.PaymentType = park.PaymentType;
        existing.TotalDebitAmount = request.TotalDebitAmount;
        existing.TotalCreditAmount = request.TotalCreditAmount;
        existing.TransactionCount = request.TransactionCount;
        existing.SourceType = SourceType.Manual;
        existing.SourceJobRunId = jobInfo.JobRunId;
        existing.SourceJobRunItemId = request.JobRunItemId;
        existing.RawResponseId = jobInfo.RawResponseId;
        existing.ManualReason = request.ManualReason.Trim();

        MarkJobItemResolved(jobInfo.JobRunItem, nowUtc, request.ManualReason);
        await dbContext.SaveChangesAsync(cancellationToken);

        var after = ToDto(existing, park);
        await auditService.LogAsync(new AuditLogEntry
        {
            Module = "Park",
            EntityName = "DailyBankTransactionSummary",
            EntityId = existing.Id.ToString(),
            Action = AuditAction.ManualEntry,
            Before = before,
            After = after,
        }, cancellationToken);

        return Ok(ApiResponse<DailyBankTransactionSummaryDto>.Ok(after, "Nhập tay giao dịch ngân hàng thành công."));
    }

    private async Task<JobItemResolution> ResolveJobItemAsync(int? jobRunItemId, ExternalApiSource expectedSource, CancellationToken cancellationToken)
    {
        if (jobRunItemId is null) return new JobItemResolution();
        var item = await dbContext.JobRunItems.FirstOrDefaultAsync(x => x.Id == jobRunItemId, cancellationToken);
        if (item is null) return new JobItemResolution { ErrorMessage = "Không tìm thấy lỗi job cần xử lý." };
        if (item.Source != expectedSource) return new JobItemResolution { ErrorMessage = "Loại lỗi job không khớp với dữ liệu nhập tay." };
        return new JobItemResolution { JobRunId = item.JobRunId, RawResponseId = item.RawResponseId, JobRunItem = item };
    }

    private void MarkJobItemResolved(JobRunItem? item, DateTime nowUtc, string note)
    {
        if (item is null || item.Status != JobRunItemStatus.Failed) return;
        item.Status = JobRunItemStatus.ManualResolved;
        item.ResolvedByUserId = CurrentUserId;
        item.ResolvedAtUtc = nowUtc;
        item.ManualResolutionNote = note.Trim();
    }

    private static DailyBankTransactionSummaryDto ToDto(DailyBankTransactionSummary summary, Park park)
    {
        return new DailyBankTransactionSummaryDto
        {
            Id = summary.Id,
            BusinessDate = summary.BusinessDate,
            ParkId = summary.ParkId,
            ParkCode = park.Code,
            ParkName = park.Name,
            PaymentType = summary.PaymentType,
            TransactionType = summary.TransactionType,
            TotalDebitAmount = summary.TotalDebitAmount,
            TotalCreditAmount = summary.TotalCreditAmount,
            TransactionCount = summary.TransactionCount,
            SourceType = summary.SourceType,
            SourceJobRunId = summary.SourceJobRunId,
            SourceJobRunItemId = summary.SourceJobRunItemId,
            RawResponseId = summary.RawResponseId,
            ManualReason = summary.ManualReason,
            CreatedAtUtc = summary.CreatedAtUtc,
            UpdatedAtUtc = summary.UpdatedAtUtc,
        };
    }
}

internal sealed class JobItemResolution
{
    public int? JobRunId { get; init; }
    public int? RawResponseId { get; init; }
    public JobRunItem? JobRunItem { get; init; }
    public string? ErrorMessage { get; init; }
}
