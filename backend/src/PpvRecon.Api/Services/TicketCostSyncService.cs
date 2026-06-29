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

    /// <summary>Số dòng vé bị bỏ qua do MaKhuVuiChoi không khớp Mã KVC con nào.</summary>
    public int SkippedUnmatched { get; set; }

    /// <summary>Các MaKhuVuiChoi không khớp Mã KVC con nào (để cảnh báo người dùng bổ sung KVC con).</summary>
    public List<string> UnmatchedParkCodes { get; set; } = new();
}

public interface ITicketCostSyncService
{
    Task<TicketCostSyncResult> SyncTodayAsync(int? currentUserId, CancellationToken cancellationToken);
}

/// <summary>
/// Lấy chi tiết vé bán hôm nay từ Oneinventory → map MaKhuVuiChoi vào Mã KVC con (ParkTicketType.Code) →
/// lần ra KVC cha (Park) qua ParkId để lấy PaymentType (phân tab) → gộp tổng tiền theo KVC cha
/// (mỗi KVC cha = 1 dòng) → ghi đè dữ liệu nguồn API của ngày hôm nay vào bảng TicketSaleCostDetails.
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

        // Map Mã KVC con (ParkTicketType.Code) → ParkId của KVC cha.
        // Một Mã KVC con có thể ứng nhiều dòng loại vé nhưng đều thuộc cùng 1 KVC cha.
        var childTypes = await dbContext.ParkTicketTypes
            .Where(t => !t.IsDeleted)
            .Select(t => new { t.Code, t.ParkId })
            .ToListAsync(cancellationToken);
        var parentIdByChildCode = childTypes
            .Where(t => !string.IsNullOrWhiteSpace(t.Code))
            .GroupBy(t => t.Code.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().ParkId, StringComparer.OrdinalIgnoreCase);

        // KVC cha (Park): lấy PaymentType (phân tab) + snapshot mã/tên cha.
        var parks = await dbContext.Parks
            .Where(p => !p.IsDeleted)
            .Select(p => new { p.Id, p.Code, p.Name, p.PaymentType })
            .ToListAsync(cancellationToken);
        var parkById = parks.ToDictionary(p => p.Id);

        // Gộp theo MaKhuVuiChoi (= Mã KVC con) để đếm dòng không khớp cho gọn.
        var groups = apiResult.Lines
            .Where(l => !string.IsNullOrWhiteSpace(l.MaKhuVuiChoi))
            .GroupBy(l => l.MaKhuVuiChoi!.Trim());

        var unmatched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // Cộng dồn theo KVC cha (ParkId) sau khi quy các KVC con về cha.
        var byParent = new Dictionary<int, ParentAccumulator>();
        var nowUtc = DateTime.UtcNow;

        foreach (var group in groups)
        {
            var childCode = group.Key;
            if (!parentIdByChildCode.TryGetValue(childCode, out var parentId)
                || !parkById.TryGetValue(parentId, out var park))
            {
                // Không khớp Mã KVC con nào (hoặc KVC cha không còn) → bỏ qua, báo lại.
                unmatched.Add(childCode);
                result.SkippedUnmatched += group.Count();
                continue;
            }

            if (!byParent.TryGetValue(parentId, out var acc))
            {
                acc = new ParentAccumulator
                {
                    ParkId = park.Id,
                    PaymentType = park.PaymentType,
                    ParkCode = park.Code,
                    ParkName = park.Name,
                };
                byParent[parentId] = acc;
            }

            acc.SalesAmount += group.Sum(l => ToLong(l.TienBan));
            acc.CostAmount += group.Sum(l => ToLong(l.TienVon));
            acc.Subtotal += group.Sum(l => ToLong(l.TamTinh));
            acc.Quantity += group.Sum(l => ToInt(l.SoLuongVe));
        }

        var aggregated = byParent.Values.Select(acc => new TicketSaleCostDetail
        {
            BusinessDate = today,
            ParkId = acc.ParkId,
            PaymentType = acc.PaymentType,
            ParkCodeSnapshot = acc.ParkCode,
            ParkNameSnapshot = acc.ParkName,

            // Các giá trị tiền đã gộp theo KVC cha/ngày.
            SalesAmount = acc.SalesAmount,
            CostAmount = acc.CostAmount,
            Subtotal = acc.Subtotal,
            Quantity = acc.Quantity,

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
        }).ToList();

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

    /// <summary>Bộ cộng dồn tổng tiền vé của các KVC con quy về một KVC cha trong ngày.</summary>
    private sealed class ParentAccumulator
    {
        public int ParkId { get; init; }
        public ParkPaymentType PaymentType { get; init; }
        public string ParkCode { get; init; } = string.Empty;
        public string ParkName { get; init; } = string.Empty;
        public long SalesAmount { get; set; }
        public long CostAmount { get; set; }
        public long Subtotal { get; set; }
        public int Quantity { get; set; }
    }
}
