using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Api.Services;
using PpvRecon.Application.Agencies;
using PpvRecon.Application.Common;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

/// <summary>
/// API cho màn "Giao dịch của các đại lý trên AR": đọc dữ liệu đã đồng bộ từ AR trong DB
/// (phân trang, lọc ngày/đại lý, tìm mã booking, tổng tiền) và cho phép đồng bộ tay theo ngày.
/// </summary>
[ApiController]
[Authorize]
[Route("api/agency-ar-transactions")]
public sealed class AgencyArTransactionsController(
    PpvReconDbContext dbContext,
    IArTransactionSyncService syncService) : PpvControllerBase
{
    // AR không có DST; giờ tường lưu trong DB được gắn offset +07:00 khi trả về API.
    private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AgencyArTransactionListResult>>> List(
        [FromQuery] int page = 1,
        [FromQuery] DateOnly? dateFrom = null,
        [FromQuery] DateOnly? dateTo = null,
        [FromQuery] string? travelAgentCode = null,
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var pageSize = PagedResult<AgencyArTransactionDto>.FixedPageSize;

        var query = dbContext.AgencyArTransactions.AsNoTracking();

        if (dateFrom is not null) query = query.Where(x => x.BusinessDate >= dateFrom);
        if (dateTo is not null) query = query.Where(x => x.BusinessDate <= dateTo);

        if (!string.IsNullOrWhiteSpace(travelAgentCode))
        {
            var code = travelAgentCode.Trim();
            query = query.Where(x => x.TravelAgentCode == code);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(x =>
                EF.Functions.Like(x.BookingId, $"%{kw}%")
                || (x.TravelAgentCode != null && EF.Functions.Like(x.TravelAgentCode, $"%{kw}%"))
                || (x.TravelAgentName != null && EF.Functions.Like(x.TravelAgentName, $"%{kw}%")));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var totalAmount = totalItems == 0 ? 0L : await query.SumAsync(x => x.Amount, cancellationToken);

        // Lấy bản ghi rồi map trong bộ nhớ để gắn offset +07:00 cho TransactionDate (không dựng
        // DateTimeOffset trong SQL). ORDER BY dùng DateTime nên SQLite hỗ trợ.
        var rows = await query
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = rows.Select(x => new AgencyArTransactionDto
        {
            Id = x.Id,
            BookingId = x.BookingId,
            TravelAgentName = x.TravelAgentName,
            TransactionDate = new DateTimeOffset(DateTime.SpecifyKind(x.TransactionDate, DateTimeKind.Unspecified), VnOffset),
            Amount = x.Amount,
            TravelAgentCode = x.TravelAgentCode,
            ReceivableAccountCode = x.ReceivableAccountCode,
            Description = x.Description,
            BusinessDate = x.BusinessDate,
            SyncedAtUtc = x.UpdatedAtUtc ?? x.CreatedAtUtc,
        }).ToList();

        return Ok(ApiResponse<AgencyArTransactionListResult>.Ok(new AgencyArTransactionListResult
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            TotalAmount = totalAmount,
        }));
    }

    public sealed class ArSyncRequest
    {
        /// <summary>Ngày cần lấy dữ liệu; bỏ trống = hôm nay (giờ VN).</summary>
        public DateOnly? BusinessDate { get; set; }
    }

    /// <summary>
    /// Lấy giao dịch đại lý trên AR của ngày chỉ định (mặc định hôm nay) từ AR và upsert vào DB.
    /// Chỉ Admin/Kế toán được chạy tay (§15).
    /// </summary>
    [HttpPost("sync")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<ApiResponse<AgencyArTransactionSyncResult>>> Sync(
        ArSyncRequest? request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var businessDate = request?.BusinessDate ?? GetVietnamToday();
            var result = await syncService.SyncAsync(businessDate, CurrentUserId, cancellationToken);

            var message = result.ValidBookingTransactions == 0
                ? $"Không có giao dịch 'Thanh toán tiền cho booking' cho ngày {result.BusinessDate:dd/MM/yyyy}."
                : $"Đã đồng bộ ngày {result.BusinessDate:dd/MM/yyyy}: {result.Inserted} thêm mới, {result.Updated} cập nhật, {result.Unchanged} không đổi (từ {result.ValidBookingTransactions} giao dịch booking hợp lệ).";
            if (result.ErrorRows > 0)
                message += $" {result.ErrorRows} dòng lỗi bị bỏ qua (xem log).";

            return Ok(ApiResponse<AgencyArTransactionSyncResult>.Ok(result, message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AgencyArTransactionSyncResult>.Fail(
                $"Không lấy được giao dịch đại lý trên AR: {ex.Message}"));
        }
    }
}
