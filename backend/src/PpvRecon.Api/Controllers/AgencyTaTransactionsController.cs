using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Api.Services;
using PpvRecon.Application.Agencies;
using PpvRecon.Application.Common;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

/// <summary>
/// API cho màn "Giao dịch của các đại lý trên TA": đọc dữ liệu booking đã đồng bộ từ OneInventory
/// trong DB (phân trang, lọc ngày/đại lý, tìm mã booking, tổng tiền) và cho phép đồng bộ tay theo ngày.
/// </summary>
[ApiController]
[Authorize]
[Route("api/agency-ta-transactions")]
public sealed class AgencyTaTransactionsController(
    PpvReconDbContext dbContext,
    IAgencyBookingSyncService syncService) : PpvControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<AgencyBookingListResult>>> List(
        [FromQuery] int page = 1,
        [FromQuery] DateOnly? dateFrom = null,
        [FromQuery] DateOnly? dateTo = null,
        [FromQuery] int? agencyId = null,
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var pageSize = PagedResult<AgencyBookingDto>.FixedPageSize;

        var query = dbContext.AgencyBookings.AsNoTracking();

        if (dateFrom is not null) query = query.Where(x => x.BookingDate >= dateFrom);
        if (dateTo is not null) query = query.Where(x => x.BookingDate <= dateTo);
        if (agencyId is not null) query = query.Where(x => x.BuyingAgentId == agencyId);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(x =>
                EF.Functions.Like(x.BookingCode, $"%{kw}%")
                || EF.Functions.Like(x.BuyingAgentCode, $"%{kw}%")
                || (x.BuyingAgentName != null && EF.Functions.Like(x.BuyingAgentName, $"%{kw}%")));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        // §12: tổng "Số tiền" (TienBan) trên toàn bộ tập đã lọc.
        var totalAmount = totalItems == 0
            ? 0L
            : await query.SumAsync(x => x.SalesAmount, cancellationToken);

        var items = await query
            .OrderByDescending(x => x.BookingDate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AgencyBookingDto
            {
                Id = x.Id,
                BookingCode = x.BookingCode,
                BookingDate = x.BookingDate,
                BuyingAgentId = x.BuyingAgentId,
                IsAgencyMatched = x.IsAgencyMatched,
                BuyingAgentCode = x.BuyingAgentCode,
                BuyingAgentName = x.BuyingAgentName,
                ParentBuyingAgentCode = x.ParentBuyingAgentCode,
                ParentBuyingAgentName = x.ParentBuyingAgentName,
                SellingAgentCode = x.SellingAgentCode,
                SellingAgentName = x.SellingAgentName,
                ParkExternalCode = x.ParkExternalCode,
                ParkExternalName = x.ParkExternalName,
                TicketTypeCode = x.TicketTypeCode,
                TicketTypeName = x.TicketTypeName,
                TicketGroupName = x.TicketGroupName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                SalesAmount = x.SalesAmount,
                CostAmount = x.CostAmount,
                Subtotal = x.Subtotal,
                Discount = x.Discount,
                SourceSystem = x.SourceSystem,
                SourceTransactionId = x.SourceTransactionId,
                SourceType = x.SourceType,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc,
                SyncedAtUtc = x.UpdatedAtUtc ?? x.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<AgencyBookingListResult>.Ok(new AgencyBookingListResult
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            TotalAmount = totalAmount,
        }));
    }

    public sealed class AgencyBookingSyncRequest
    {
        /// <summary>Ngày cần lấy dữ liệu; bỏ trống = hôm nay (giờ VN).</summary>
        public DateOnly? BusinessDate { get; set; }
    }

    /// <summary>
    /// Lấy giao dịch đại lý trên TA của ngày chỉ định (mặc định hôm nay) từ OneInventory và upsert vào DB.
    /// Chỉ Admin/Ke toán được chạy tay (§13).
    /// </summary>
    [HttpPost("sync")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<ApiResponse<AgencyBookingSyncResult>>> Sync(
        AgencyBookingSyncRequest? request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var businessDate = request?.BusinessDate ?? GetVietnamToday();
            var result = await syncService.SyncAsync(businessDate, CurrentUserId, cancellationToken);

            var message = result.MatchedParent == 0
                ? $"Không có giao dịch đại lý (mã cấp trên {result.ParentAgencyCode}) cho ngày {result.BusinessDate:dd/MM/yyyy}."
                : $"Đã đồng bộ ngày {result.BusinessDate:dd/MM/yyyy}: {result.Inserted} thêm mới, {result.Updated} cập nhật, {result.Skipped} không đổi (trên {result.TotalLines} dòng API).";
            if (result.UnmatchedAgency > 0)
                message += $" {result.UnmatchedAgency} dòng chưa map được đại lý: {string.Join(", ", result.UnmatchedAgencyCodes)}.";

            return Ok(ApiResponse<AgencyBookingSyncResult>.Ok(result, message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AgencyBookingSyncResult>.Fail(
                $"Không lấy được giao dịch đại lý trên TA: {ex.Message}"));
        }
    }
}
