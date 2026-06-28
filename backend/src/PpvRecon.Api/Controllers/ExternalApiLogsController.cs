using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Common;
using PpvRecon.Application.Jobs;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/external-api-logs")]
public sealed class ExternalApiLogsController(PpvReconDbContext dbContext) : PpvControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ExternalApiLogListItemDto>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] ExternalApiSource? source = null,
        [FromQuery] bool? isSuccess = null,
        [FromQuery] int? parkId = null,
        [FromQuery] string? keyword = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query = dbContext.ExternalApiRawResponses.AsNoTracking();

        if (source is not null)
        {
            query = query.Where(x => x.Source == source);
        }

        if (isSuccess is not null)
        {
            query = query.Where(x => x.IsSuccess == isSuccess);
        }

        if (parkId is not null)
        {
            query = query.Where(x => x.ParkId == parkId);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(x =>
                (x.RequestUrl != null && x.RequestUrl.Contains(term))
                || (x.ErrorMessage != null && x.ErrorMessage.Contains(term)));
        }

        if (fromDate is not null)
        {
            query = query.Where(x => x.ReceivedAtUtc >= fromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        if (toDate is not null)
        {
            query = query.Where(x => x.ReceivedAtUtc < toDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.ReceivedAtUtc)
            .Skip((page - 1) * PagedResult<ExternalApiLogListItemDto>.FixedPageSize)
            .Take(PagedResult<ExternalApiLogListItemDto>.FixedPageSize)
            .Select(x => new ExternalApiLogListItemDto
            {
                Id = x.Id,
                Source = x.Source,
                BusinessDate = x.BusinessDate,
                ParkId = x.ParkId,
                ParkName = x.ParkId == null
                    ? null
                    : dbContext.Parks.Where(p => p.Id == x.ParkId).Select(p => p.Name).FirstOrDefault(),
                JobRunId = x.JobRunId,
                RequestUrl = x.RequestUrl,
                ResponseStatusCode = x.ResponseStatusCode,
                IsSuccess = x.IsSuccess,
                ErrorMessage = x.ErrorMessage,
                DurationMs = x.DurationMs,
                ReceivedAtUtc = x.ReceivedAtUtc,
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ExternalApiLogListItemDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<ExternalApiLogListItemDto>.FixedPageSize),
        };

        return Ok(ApiResponse<PagedResult<ExternalApiLogListItemDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ExternalApiLogDetailDto>>> Detail(
        int id,
        CancellationToken cancellationToken)
    {
        var log = await dbContext.ExternalApiRawResponses
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ExternalApiLogDetailDto
            {
                Id = x.Id,
                Source = x.Source,
                BusinessDate = x.BusinessDate,
                ParkId = x.ParkId,
                ParkName = x.ParkId == null
                    ? null
                    : dbContext.Parks.Where(p => p.Id == x.ParkId).Select(p => p.Name).FirstOrDefault(),
                JobRunId = x.JobRunId,
                RequestUrl = x.RequestUrl,
                ResponseStatusCode = x.ResponseStatusCode,
                IsSuccess = x.IsSuccess,
                ErrorMessage = x.ErrorMessage,
                DurationMs = x.DurationMs,
                ReceivedAtUtc = x.ReceivedAtUtc,
                RequestPayloadJson = x.RequestPayloadJson,
                ResponseBodyJson = x.ResponseBodyJson,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (log is null)
        {
            return NotFound(ApiResponse<ExternalApiLogDetailDto>.Fail("Không tìm thấy log gọi API."));
        }

        return Ok(ApiResponse<ExternalApiLogDetailDto>.Ok(log));
    }
}
