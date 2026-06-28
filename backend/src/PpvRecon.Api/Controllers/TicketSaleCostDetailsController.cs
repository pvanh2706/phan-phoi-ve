using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Common;
using PpvRecon.Application.Summaries;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/ticket-cost-details")]
public sealed class TicketSaleCostDetailsController(PpvReconDbContext dbContext) : PpvControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TicketSaleCostDetailDto>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] DateOnly? dateFrom = null,
        [FromQuery] DateOnly? dateTo = null,
        [FromQuery] int? parkId = null,
        [FromQuery] ParkPaymentType? paymentType = null,
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query = dbContext.TicketSaleCostDetails.AsNoTracking();

        if (dateFrom is not null) query = query.Where(x => x.BusinessDate >= dateFrom);
        if (dateTo is not null) query = query.Where(x => x.BusinessDate <= dateTo);
        if (parkId is not null) query = query.Where(x => x.ParkId == parkId);
        if (paymentType is not null) query = query.Where(x => x.PaymentType == paymentType);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(x =>
                EF.Functions.Like(x.ParkNameSnapshot, $"%{kw}%")
                || EF.Functions.Like(x.ParkCodeSnapshot, $"%{kw}%")
                || EF.Functions.Like(x.BookingCode, $"%{kw}%")
                || EF.Functions.Like(x.TicketTypeName, $"%{kw}%")
                || (x.TicketGroupName != null && EF.Functions.Like(x.TicketGroupName, $"%{kw}%"))
                || (x.BuyingAgentName != null && EF.Functions.Like(x.BuyingAgentName, $"%{kw}%"))
                || (x.SellingAgentName != null && EF.Functions.Like(x.SellingAgentName, $"%{kw}%")));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.BusinessDate)
            .ThenBy(x => x.BookingCode)
            .Skip((page - 1) * PagedResult<TicketSaleCostDetailDto>.FixedPageSize)
            .Take(PagedResult<TicketSaleCostDetailDto>.FixedPageSize)
            .Select(x => new TicketSaleCostDetailDto
            {
                Id = x.Id,
                BusinessDate = x.BusinessDate,
                ParkId = x.ParkId,
                PaymentType = x.PaymentType,
                BookingCode = x.BookingCode,
                UnitPrice = x.UnitPrice,
                TicketTypeName = x.TicketTypeName,
                TicketGroupName = x.TicketGroupName,
                SalesAmount = x.SalesAmount,
                CostAmount = x.CostAmount,
                SellingAgentCode = x.SellingAgentCode,
                Quantity = x.Quantity,
                BuyingAgentCode = x.BuyingAgentCode,
                BuyingAgentName = x.BuyingAgentName,
                ParkCodeSnapshot = x.ParkCodeSnapshot,
                ParkNameSnapshot = x.ParkNameSnapshot,
                Subtotal = x.Subtotal,
                ExternalLineId = x.ExternalLineId,
                SellingAgentName = x.SellingAgentName,
                TicketTypeCode = x.TicketTypeCode,
                ParentBuyingAgentName = x.ParentBuyingAgentName,
                ParentBuyingAgentCode = x.ParentBuyingAgentCode,
                SourceType = x.SourceType,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<TicketSaleCostDetailDto>>.Ok(new PagedResult<TicketSaleCostDetailDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<TicketSaleCostDetailDto>.FixedPageSize),
        }));
    }
}
