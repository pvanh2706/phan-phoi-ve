using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Common;
using PpvRecon.Application.Jobs;
using PpvRecon.Api.Services;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/jobs")]
public sealed class JobsController(
    PpvReconDbContext dbContext,
    IJobRunner jobRunner,
    IMaintenanceJobService maintenanceJobService) : PpvControllerBase
{
    [HttpGet("runs")]
    public async Task<ActionResult<ApiResponse<PagedResult<JobRunListItemDto>>>> ListRuns(
        [FromQuery] int page = 1,
        [FromQuery] string? jobName = null,
        [FromQuery] DateOnly? businessDate = null,
        [FromQuery] JobRunStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query = dbContext.JobRuns.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(jobName))
        {
            query = query.Where(x => x.JobName == jobName.Trim());
        }

        if (businessDate is not null)
        {
            query = query.Where(x => x.BusinessDate == businessDate);
        }

        if (status is not null)
        {
            query = query.Where(x => x.Status == status);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.StartedAtUtc)
            .Skip((page - 1) * PagedResult<JobRunListItemDto>.FixedPageSize)
            .Take(PagedResult<JobRunListItemDto>.FixedPageSize)
            .Select(x => new JobRunListItemDto
            {
                Id = x.Id,
                JobName = x.JobName,
                BusinessDate = x.BusinessDate,
                TriggeredBy = x.TriggeredBy,
                TriggeredByUserId = x.TriggeredByUserId,
                StartedAtUtc = x.StartedAtUtc,
                FinishedAtUtc = x.FinishedAtUtc,
                Status = x.Status,
                TotalItems = x.TotalItems,
                SuccessItems = x.SuccessItems,
                FailedItems = x.FailedItems,
                SkippedItems = x.SkippedItems,
                ErrorMessage = x.ErrorMessage,
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<JobRunListItemDto>>.Ok(new PagedResult<JobRunListItemDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<JobRunListItemDto>.FixedPageSize),
        }));
    }

    [HttpGet("runs/{id:int}")]
    public async Task<ActionResult<ApiResponse<JobRunDetailDto>>> GetRun(int id, CancellationToken cancellationToken)
    {
        var run = await dbContext.JobRuns.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (run is null)
        {
            return NotFound(ApiResponse<JobRunDetailDto>.Fail("Không tìm thấy lần chạy job."));
        }

        var items = await BuildJobRunItemsQuery()
            .Where(x => x.JobRunId == id)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<JobRunDetailDto>.Ok(new JobRunDetailDto
        {
            Id = run.Id,
            JobName = run.JobName,
            BusinessDate = run.BusinessDate,
            TriggeredBy = run.TriggeredBy,
            TriggeredByUserId = run.TriggeredByUserId,
            StartedAtUtc = run.StartedAtUtc,
            FinishedAtUtc = run.FinishedAtUtc,
            Status = run.Status,
            TotalItems = run.TotalItems,
            SuccessItems = run.SuccessItems,
            FailedItems = run.FailedItems,
            SkippedItems = run.SkippedItems,
            ErrorMessage = run.ErrorMessage,
            SummaryJson = run.SummaryJson,
            Items = items,
        }));
    }

    [HttpGet("errors")]
    public async Task<ActionResult<ApiResponse<PagedResult<JobRunItemDto>>>> ListErrors(
        [FromQuery] int page = 1,
        [FromQuery] DateOnly? businessDate = null,
        [FromQuery] ExternalApiSource? source = null,
        [FromQuery] JobRunItemStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query = BuildJobRunItemsQuery()
            .Where(x => x.Status == JobRunItemStatus.Failed || x.Status == JobRunItemStatus.ManualResolved);

        if (businessDate is not null)
        {
            query = query.Where(x => x.BusinessDate == businessDate);
        }

        if (source is not null)
        {
            query = query.Where(x => x.Source == source);
        }

        if (status is not null)
        {
            query = query.Where(x => x.Status == status);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.StartedAtUtc)
            .Skip((page - 1) * PagedResult<JobRunItemDto>.FixedPageSize)
            .Take(PagedResult<JobRunItemDto>.FixedPageSize)
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<JobRunItemDto>>.Ok(new PagedResult<JobRunItemDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<JobRunItemDto>.FixedPageSize),
        }));
    }

    [HttpPost("sync-park-balances/run")]
    public Task<ActionResult<ApiResponse<JobRunDetailDto>>> RunParkBalances(RunJobRequest request, CancellationToken cancellationToken)
    {
        return RunExternalSyncAsync(ExternalApiSource.ParkBalance, request, cancellationToken);
    }

    [HttpPost("sync-ticket-costs/run")]
    public Task<ActionResult<ApiResponse<JobRunDetailDto>>> RunTicketCosts(RunJobRequest request, CancellationToken cancellationToken)
    {
        return RunExternalSyncAsync(ExternalApiSource.TicketCost, request, cancellationToken);
    }

    [HttpPost("sync-bank-transactions/run")]
    public Task<ActionResult<ApiResponse<JobRunDetailDto>>> RunBankTransactions(RunJobRequest request, CancellationToken cancellationToken)
    {
        return RunExternalSyncAsync(ExternalApiSource.BankTransaction, request, cancellationToken);
    }

    [Authorize(Roles = "Admin,Accountant")]
    [HttpPost("send-sync-error-summary/run")]
    public async Task<ActionResult<ApiResponse<SendSyncErrorSummaryResultDto>>> SendSyncErrorSummary(
        RunJobRequest request,
        CancellationToken cancellationToken)
    {
        var businessDate = request.BusinessDate ?? GetVietnamToday();
        var result = await maintenanceJobService.SendSyncErrorSummaryAsync(businessDate, CurrentUserId, cancellationToken);
        return Ok(ApiResponse<SendSyncErrorSummaryResultDto>.Ok(result, "Đã chạy job gửi tổng hợp lỗi đồng bộ."));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("cleanup-audit-logs/run")]
    public async Task<ActionResult<ApiResponse<CleanupAuditLogsResultDto>>> CleanupAuditLogs(CancellationToken cancellationToken)
    {
        var result = await maintenanceJobService.CleanupAuditLogsAsync(CurrentUserId, cancellationToken);
        return Ok(ApiResponse<CleanupAuditLogsResultDto>.Ok(result, "Đã chạy job dọn audit log."));
    }

    private async Task<ActionResult<ApiResponse<JobRunDetailDto>>> RunExternalSyncAsync(
        ExternalApiSource source,
        RunJobRequest request,
        CancellationToken cancellationToken)
    {
        var businessDate = request.BusinessDate ?? GetVietnamToday();
        var result = await jobRunner.RunExternalSyncAsync(
            source,
            businessDate,
            JobTriggerType.Manual,
            CurrentUserId,
            cancellationToken);

        return Ok(ApiResponse<JobRunDetailDto>.Ok(result, "Đã chạy job đồng bộ."));
    }

    private IQueryable<JobRunItemDto> BuildJobRunItemsQuery()
    {
        return
            from item in dbContext.JobRunItems.AsNoTracking()
            join park in dbContext.Parks.AsNoTracking() on item.ParkId equals park.Id into parkGroup
            from park in parkGroup.DefaultIfEmpty()
            select new JobRunItemDto
            {
                Id = item.Id,
                JobRunId = item.JobRunId,
                BusinessDate = item.BusinessDate,
                ParkId = item.ParkId,
                ParkCode = park != null ? park.Code : null,
                ParkName = park != null ? park.Name : null,
                Source = item.Source,
                Status = item.Status,
                AttemptCount = item.AttemptCount,
                StartedAtUtc = item.StartedAtUtc,
                FinishedAtUtc = item.FinishedAtUtc,
                DurationMs = item.DurationMs,
                ErrorCode = item.ErrorCode,
                ErrorMessage = item.ErrorMessage,
                RawResponseId = item.RawResponseId,
                ResolvedByUserId = item.ResolvedByUserId,
                ResolvedAtUtc = item.ResolvedAtUtc,
                ManualResolutionNote = item.ManualResolutionNote,
            };
    }

}
