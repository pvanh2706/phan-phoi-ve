using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Domain.Entities.Summaries;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Services;

/// <summary>Kết quả một lần "Lấy dữ liệu" giá vốn vé bán từ Oneinventory.</summary>
public sealed class TicketCostSyncResult
{
    public DateOnly BusinessDate { get; set; }

    /// <summary>Tổng số dòng vé thô API trả về (trước khi gộp).</summary>
    public int TotalLines { get; set; }

    /// <summary>Số dòng tổng hợp đã ghi (mỗi KVC = 1 dòng).</summary>
    public int Imported { get; set; }

    /// <summary>Số dòng vé bị bỏ qua do KVC không khớp Park.Code.</summary>
    public int SkippedUnmatched { get; set; }

    /// <summary>Các MaKhuVuiChoi không khớp Park nào (để cảnh báo người dùng bổ sung KVC).</summary>
    public List<string> UnmatchedParkCodes { get; set; } = new();
}

public interface ITicketCostSyncService
{
    Task<TicketCostSyncResult> SyncTodayAsync(int? currentUserId, CancellationToken cancellationToken);
}

/// <summary>
/// Lấy chi tiết vé bán hôm nay từ Oneinventory → gộp theo MaKhuVuiChoi (cộng tổng tiền) →
/// map Park qua Code để lấy ParkId + PaymentType (phân tab) →
/// ghi đè dữ liệu nguồn API của ngày hôm nay vào bảng TicketSaleCostDetails.
/// </summary>
public sealed class TicketCostSyncService(
    IOneInventoryBookingApiClient apiClient,
    PpvReconDbContext dbContext) : ITicketCostSyncService
{
    public async Task<TicketCostSyncResult> SyncTodayAsync(int? currentUserId, CancellationToken cancellationToken)
    {
        var today = VietnamToday();
        var result = new TicketCostSyncResult { BusinessDate = today };

        var apiResult = await apiClient.FetchBookingsAsync(today, cancellationToken);
        if (!apiResult.IsSuccess)
        {
            throw new InvalidOperationException(apiResult.ErrorMessage ?? "Không lấy được dữ liệu từ Oneinventory.");
        }

        result.TotalLines = apiResult.Lines.Count;

        // Map Park theo Code (MaKhuVuiChoi == Park.Code).
        var parks = await dbContext.Parks
            .Where(p => !p.IsDeleted)
            .Select(p => new { p.Id, p.Code, p.Name, p.PaymentType })
            .ToListAsync(cancellationToken);
        var parkByCode = parks
            .GroupBy(p => p.Code.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // Gộp theo MaKhuVuiChoi trong ngày: cộng tổng TienBan / TienVon / TamTinh / SoLuongVe.
        var groups = apiResult.Lines
            .Where(l => !string.IsNullOrWhiteSpace(l.MaKhuVuiChoi))
            .GroupBy(l => l.MaKhuVuiChoi!.Trim());

        var unmatched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var aggregated = new List<TicketSaleCostDetail>();
        var nowUtc = DateTime.UtcNow;

        foreach (var group in groups)
        {
            var parkCode = group.Key;
            if (!parkByCode.TryGetValue(parkCode, out var park))
            {
                // Không khớp Park.Code → không xác định được tab (PaymentType) nên bỏ qua, báo lại.
                unmatched.Add(parkCode);
                result.SkippedUnmatched += group.Count();
                continue;
            }

            var parkName = group.Select(l => l.TenKhuVuiChoi).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                ?? park.Name;

            aggregated.Add(new TicketSaleCostDetail
            {
                BusinessDate = today,
                ParkId = park.Id,
                PaymentType = park.PaymentType,
                ParkCodeSnapshot = parkCode,
                ParkNameSnapshot = parkName,

                // Các giá trị tiền đã gộp theo KVC/ngày.
                SalesAmount = group.Sum(l => ToLong(l.TienBan)),
                CostAmount = group.Sum(l => ToLong(l.TienVon)),
                Subtotal = group.Sum(l => ToLong(l.TamTinh)),
                Quantity = group.Sum(l => ToInt(l.SoLuongVe)),

                // TODO(map lại): các cột chi tiết theo từng dòng vé không còn ý nghĩa khi gộp theo KVC/ngày.
                // Đang để trống — anh tự map lại trong code nếu muốn hiển thị giá trị khác.
                BookingCode = string.Empty,        // Mã đặt vé (per-line) - để trống
                UnitPrice = 0,                     // Đơn giá (per-line) - để trống
                TicketTypeName = string.Empty,     // Tên loại vé (per-line) - để trống
                TicketGroupName = null,            // Tên nhóm loại vé (per-line) - để trống
                SellingAgentCode = null,           // Mã ĐL bán (per-line) - để trống
                BuyingAgentCode = null,            // Mã ĐL mua (per-line) - để trống
                BuyingAgentName = null,            // Tên đại lý mua (per-line) - để trống
                ExternalLineId = null,             // ID (per-line) - để trống
                SellingAgentName = null,           // Tên đại lý bán (per-line) - để trống
                TicketTypeCode = null,             // Mã loại vé (per-line) - để trống
                ParentBuyingAgentName = null,      // Tên ĐL mua cấp trên (per-line) - để trống
                ParentBuyingAgentCode = null,      // Mã ĐL mua cấp trên (per-line) - để trống

                SourceType = SourceType.Api,
                CreatedAtUtc = nowUtc,
                CreatedByUserId = currentUserId,
            });
        }

        result.UnmatchedParkCodes = unmatched.ToList();

        // Ghi đè: xóa dữ liệu nguồn API của hôm nay rồi nạp lại bản gộp mới.
        var existing = await dbContext.TicketSaleCostDetails
            .Where(x => x.SourceType == SourceType.Api && x.BusinessDate == today)
            .ToListAsync(cancellationToken);
        dbContext.TicketSaleCostDetails.RemoveRange(existing);

        if (aggregated.Count > 0)
        {
            await dbContext.TicketSaleCostDetails.AddRangeAsync(aggregated, cancellationToken);
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        result.Imported = aggregated.Count;
        return result;
    }

    private static DateOnly VietnamToday()
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
        }
        catch (TimeZoneNotFoundException)
        {
            return DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        }
    }

    private static long ToLong(string? value)
        => long.TryParse((value ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v
            : 0;

    private static int ToInt(string? value)
        => int.TryParse((value ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v
            : 0;
}
