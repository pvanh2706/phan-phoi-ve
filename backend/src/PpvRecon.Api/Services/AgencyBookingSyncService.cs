using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Api.Services.Settings;
using PpvRecon.Application.Agencies;
using PpvRecon.Domain.Entities.Agencies;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Services;

public interface IAgencyBookingSyncService
{
    /// <summary>
    /// Lấy giao dịch (booking) của các đại lý trên TA cho đúng 1 ngày từ OneInventory, lọc theo
    /// mã đại lý cấp trên (§7), map đại lý nội bộ (§9) và upsert chống trùng theo ID nguồn (§10).
    /// </summary>
    Task<AgencyBookingSyncResult> SyncAsync(DateOnly businessDate, int? currentUserId, CancellationToken cancellationToken);
}

/// <summary>
/// Đồng bộ "Giao dịch của các đại lý trên TA":
/// 1) Gọi OneInventory rp_booking_list cho ngày chỉ định (tái dùng <see cref="IOneInventoryBookingApiClient"/>).
/// 2) Chỉ giữ dòng có MaDaiLyMuaCapTren = ParentAgencyCode (cấu hình, không hard-code).
/// 3) Map MaDaiLyMua với Agency.Code để lấy khóa nội bộ; không thấy thì để NULL + ghi nhận chưa map.
/// 4) Upsert theo (SourceSystem + ID) nên chạy lại cùng ngày không tạo bản ghi trùng.
/// </summary>
public sealed class AgencyBookingSyncService(
    IOneInventoryBookingApiClient apiClient,
    IConnectionSettingsService connectionSettings,
    PpvReconDbContext dbContext) : IAgencyBookingSyncService
{
    private const string SourceSystemName = "OneInventory";

    public async Task<AgencyBookingSyncResult> SyncAsync(
        DateOnly businessDate, int? currentUserId, CancellationToken cancellationToken)
    {
        var options = await connectionSettings.GetOneInventoryAsync(cancellationToken);
        var parentCode = string.IsNullOrWhiteSpace(options.ParentAgencyCode)
            ? "5129"
            : options.ParentAgencyCode.Trim();

        var result = new AgencyBookingSyncResult { BusinessDate = businessDate, ParentAgencyCode = parentCode };

        var apiResult = await apiClient.FetchBookingsAsync(businessDate, cancellationToken);
        if (!apiResult.IsSuccess)
        {
            throw new InvalidOperationException(apiResult.ErrorMessage ?? "Không lấy được dữ liệu từ Oneinventory.");
        }

        result.TotalLines = apiResult.Lines.Count;

        // §7: chỉ giữ dòng có MaDaiLyMuaCapTren khớp mã cấu hình và có ID nguồn.
        // Nếu API trả trùng ID trong cùng một lần, lấy bản ghi cuối để tránh xung đột unique.
        var filtered = apiResult.Lines
            .Where(l => string.Equals((l.MaDaiLyMuaCapTren ?? string.Empty).Trim(), parentCode, StringComparison.OrdinalIgnoreCase))
            .Where(l => !string.IsNullOrWhiteSpace(l.ID))
            .GroupBy(l => l.ID!.Trim(), StringComparer.Ordinal)
            .Select(g => g.Last())
            .ToList();

        result.MatchedParent = filtered.Count;
        if (filtered.Count == 0)
        {
            return result;
        }

        // §9: map MaDaiLyMua -> Agency.Id qua Agency.Code (loại đại lý đã xóa mềm).
        var agencyRows = await dbContext.Agencies
            .Where(a => !a.IsDeleted)
            .Select(a => new { a.Code, a.Id })
            .ToListAsync(cancellationToken);
        var agencyIdByCode = agencyRows
            .Where(a => !string.IsNullOrWhiteSpace(a.Code))
            .GroupBy(a => a.Code.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        // §10: nạp sẵn bản ghi đã tồn tại theo ID nguồn để upsert (chunk tránh giới hạn tham số SQLite).
        var sourceIds = filtered.Select(l => l.ID!.Trim()).ToList();
        var existingBySourceId = new Dictionary<string, AgencyBooking>(StringComparer.Ordinal);
        foreach (var chunk in sourceIds.Chunk(500))
        {
            var rows = await dbContext.AgencyBookings
                .Where(x => x.SourceSystem == SourceSystemName && chunk.Contains(x.SourceTransactionId))
                .ToListAsync(cancellationToken);
            foreach (var row in rows)
            {
                existingBySourceId[row.SourceTransactionId] = row;
            }
        }

        var nowUtc = DateTime.UtcNow;
        var unmatched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in filtered)
        {
            var sourceId = line.ID!.Trim();
            var buyingCode = (line.MaDaiLyMua ?? string.Empty).Trim();

            int? agencyId = null;
            if (!string.IsNullOrWhiteSpace(buyingCode) && agencyIdByCode.TryGetValue(buyingCode, out var foundId))
            {
                agencyId = foundId;
            }
            else if (!string.IsNullOrWhiteSpace(buyingCode))
            {
                unmatched.Add(buyingCode);
            }

            if (agencyId is null)
            {
                result.UnmatchedAgency++;
            }

            if (existingBySourceId.TryGetValue(sourceId, out var existing))
            {
                var changed = ApplyLine(existing, line, buyingCode, agencyId, businessDate);
                if (changed)
                {
                    existing.UpdatedAtUtc = nowUtc;
                    existing.UpdatedByUserId = currentUserId;
                    result.Updated++;
                }
                else
                {
                    result.Skipped++;
                }
            }
            else
            {
                var booking = new AgencyBooking
                {
                    SourceSystem = SourceSystemName,
                    SourceTransactionId = sourceId,
                    SourceType = SourceType.Api,
                    CreatedAtUtc = nowUtc,
                    CreatedByUserId = currentUserId,
                };
                ApplyLine(booking, line, buyingCode, agencyId, businessDate);
                dbContext.AgencyBookings.Add(booking);
                result.Inserted++;
            }
        }

        result.UnmatchedAgencyCodes = unmatched.OrderBy(x => x).ToList();
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    /// <summary>
    /// Gán giá trị từ dòng nguồn vào entity, chuyển kiểu tiền/số/ngày phù hợp (§6).
    /// Trả về true nếu có ít nhất một trường thay đổi (để phân biệt Updated vs Skipped §10).
    /// </summary>
    private static bool ApplyLine(
        AgencyBooking b, OneInventoryBookingLine line, string buyingCode, int? agencyId, DateOnly businessDate)
    {
        var bookingCode = (line.MaDatVe ?? string.Empty).Trim();
        var bookingDate = ParseDate(line.NgayDat, businessDate);
        var matched = agencyId is not null;
        var buyingName = TrimOrNull(line.TenDaiLyMua);
        var parentCode = TrimOrNull(line.MaDaiLyMuaCapTren);
        var parentName = TrimOrNull(line.TenDaiLyMuaCapTren);
        var sellingCode = TrimOrNull(line.MaDaiLyBan);
        var sellingName = TrimOrNull(line.TenDaiLyBan);
        var parkCode = TrimOrNull(line.MaKhuVuiChoi);
        var parkName = TrimOrNull(line.TenKhuVuiChoi);
        var ticketCode = TrimOrNull(line.MaLoaiVe);
        var ticketName = TrimOrNull(line.TenLoaiVe);
        var groupName = TrimOrNull(line.TenNhomLoaiVe);
        var quantity = ToInt(line.SoLuongVe);
        var unitPrice = ToLong(line.DonGia);
        var salesAmount = ToLong(line.TienBan);
        var costAmount = ToLong(line.TienVon);
        var subtotal = ToLong(line.TamTinh);
        var discount = ToLong(line.GiamGia);

        var changed =
            b.BookingCode != bookingCode
            || b.BookingDate != bookingDate
            || b.BuyingAgentId != agencyId
            || b.IsAgencyMatched != matched
            || b.BuyingAgentCode != buyingCode
            || b.BuyingAgentName != buyingName
            || b.ParentBuyingAgentCode != parentCode
            || b.ParentBuyingAgentName != parentName
            || b.SellingAgentCode != sellingCode
            || b.SellingAgentName != sellingName
            || b.ParkExternalCode != parkCode
            || b.ParkExternalName != parkName
            || b.TicketTypeCode != ticketCode
            || b.TicketTypeName != ticketName
            || b.TicketGroupName != groupName
            || b.Quantity != quantity
            || b.UnitPrice != unitPrice
            || b.SalesAmount != salesAmount
            || b.CostAmount != costAmount
            || b.Subtotal != subtotal
            || b.Discount != discount;

        b.BookingCode = bookingCode;
        b.BookingDate = bookingDate;
        b.BuyingAgentId = agencyId;
        b.IsAgencyMatched = matched;
        b.BuyingAgentCode = buyingCode;
        b.BuyingAgentName = buyingName;
        b.ParentBuyingAgentCode = parentCode;
        b.ParentBuyingAgentName = parentName;
        b.SellingAgentCode = sellingCode;
        b.SellingAgentName = sellingName;
        b.ParkExternalCode = parkCode;
        b.ParkExternalName = parkName;
        b.TicketTypeCode = ticketCode;
        b.TicketTypeName = ticketName;
        b.TicketGroupName = groupName;
        b.Quantity = quantity;
        b.UnitPrice = unitPrice;
        b.SalesAmount = salesAmount;
        b.CostAmount = costAmount;
        b.Subtotal = subtotal;
        b.Discount = discount;

        return changed;
    }

    private static DateOnly ParseDate(string? value, DateOnly fallback)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            return fallback;
        }

        // NgayDat có thể là "yyyy-MM-dd" hoặc "yyyy-MM-dd HH:mm[:ss]".
        if (DateOnly.TryParseExact(trimmed, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
        {
            return dateOnly;
        }
        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            return DateOnly.FromDateTime(dateTime);
        }
        return fallback;
    }

    private static long ToLong(string? value)
        => long.TryParse((value ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v
            : 0;

    private static int ToInt(string? value)
        => int.TryParse((value ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v
            : 0;

    private static string? TrimOrNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
