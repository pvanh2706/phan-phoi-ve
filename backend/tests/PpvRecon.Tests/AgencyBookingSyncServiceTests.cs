using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Api.Services;
using PpvRecon.Api.Services.BankStatement;
using PpvRecon.Api.Services.Settings;
using PpvRecon.Domain.Entities.Agencies;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;
using Xunit;

namespace PpvRecon.Tests;

/// <summary>
/// Kiểm thử luồng đồng bộ "Giao dịch của các đại lý trên TA" (§14.11): đăng nhập/booking thành công,
/// thất bại, rỗng, sai định dạng, lọc theo mã đại lý cấp trên, chống trùng, map/không map đại lý, chạy lại.
/// Dùng SQLite in-memory để kiểm cả ràng buộc unique thật của DB.
/// </summary>
public sealed class AgencyBookingSyncServiceTests : IDisposable
{
    private static readonly DateOnly BusinessDate = new(2026, 7, 21);
    private const string ParentCode = "5129";
    private readonly List<SqliteConnection> _connections = new();

    // ── Kịch bản ──

    [Fact]
    public async Task Fetch_success_saves_matching_records()
    {
        using var ctx = CreateContext();
        var api = Success(Line("111", buyingCode: "7391"), Line("222", buyingCode: "7392"));
        var service = NewService(api, ctx);

        var result = await service.SyncAsync(BusinessDate, currentUserId: null, CancellationToken.None);

        Assert.Equal(2, result.TotalLines);
        Assert.Equal(2, result.MatchedParent);
        Assert.Equal(2, result.Inserted);
        Assert.Equal(0, result.Updated);
        Assert.Equal(2, await ctx.AgencyBookings.CountAsync());
    }

    [Fact]
    public async Task Login_failure_throws_and_saves_nothing()
    {
        using var ctx = CreateContext();
        var service = NewService(Failure("LoginFailed", "Sai tài khoản Oneinventory."), ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SyncAsync(BusinessDate, null, CancellationToken.None));
        Assert.Equal(0, await ctx.AgencyBookings.CountAsync());
    }

    [Fact]
    public async Task Booking_api_failure_throws()
    {
        using var ctx = CreateContext();
        var service = NewService(Failure("HttpRequestFailed", "API trả về HTTP 500."), ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SyncAsync(BusinessDate, null, CancellationToken.None));
    }

    [Fact]
    public async Task Empty_response_saves_nothing()
    {
        using var ctx = CreateContext();
        var service = NewService(Success(), ctx);

        var result = await service.SyncAsync(BusinessDate, null, CancellationToken.None);

        Assert.Equal(0, result.TotalLines);
        Assert.Equal(0, result.MatchedParent);
        Assert.Equal(0, await ctx.AgencyBookings.CountAsync());
    }

    [Fact]
    public async Task Malformed_response_throws()
    {
        // API client quy đổi JSON sai định dạng thành kết quả thất bại (ErrorCode=InvalidJson) → service ném lỗi.
        using var ctx = CreateContext();
        var service = NewService(Failure("InvalidJson", "Response không phải JSON hợp lệ."), ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SyncAsync(BusinessDate, null, CancellationToken.None));
    }

    [Fact]
    public async Task Record_with_matching_parent_is_saved()
    {
        using var ctx = CreateContext();
        var service = NewService(Success(Line("111", parent: ParentCode)), ctx);

        var result = await service.SyncAsync(BusinessDate, null, CancellationToken.None);

        Assert.Equal(1, result.MatchedParent);
        Assert.Equal(1, await ctx.AgencyBookings.CountAsync());
    }

    [Fact]
    public async Task Record_with_wrong_parent_is_filtered_out()
    {
        using var ctx = CreateContext();
        var service = NewService(Success(Line("111", parent: "9999")), ctx);

        var result = await service.SyncAsync(BusinessDate, null, CancellationToken.None);

        Assert.Equal(1, result.TotalLines);
        Assert.Equal(0, result.MatchedParent);
        Assert.Equal(0, await ctx.AgencyBookings.CountAsync());
    }

    [Fact]
    public async Task Mixed_parent_codes_only_matching_saved()
    {
        using var ctx = CreateContext();
        var api = Success(Line("111", parent: ParentCode), Line("222", parent: "9999"));
        var service = NewService(api, ctx);

        var result = await service.SyncAsync(BusinessDate, null, CancellationToken.None);

        Assert.Equal(2, result.TotalLines);
        Assert.Equal(1, result.MatchedParent);
        var saved = await ctx.AgencyBookings.SingleAsync();
        Assert.Equal("111", saved.SourceTransactionId);
    }

    [Fact]
    public async Task Duplicate_id_in_same_batch_saves_single_row()
    {
        using var ctx = CreateContext();
        // API trả trùng ID trong cùng một lần → chỉ 1 bản ghi (tránh vi phạm unique).
        var api = Success(Line("111", amount: "100"), Line("111", amount: "200"));
        var service = NewService(api, ctx);

        var result = await service.SyncAsync(BusinessDate, null, CancellationToken.None);

        Assert.Equal(1, result.MatchedParent);
        var saved = await ctx.AgencyBookings.SingleAsync();
        Assert.Equal(200, saved.SalesAmount); // lấy bản cuối
    }

    [Fact]
    public async Task Agency_not_found_saves_with_null_link_and_counts_unmatched()
    {
        using var ctx = CreateContext(); // không seed đại lý nào
        var service = NewService(Success(Line("111", buyingCode: "7391")), ctx);

        var result = await service.SyncAsync(BusinessDate, null, CancellationToken.None);

        Assert.Equal(1, result.UnmatchedAgency);
        Assert.Contains("7391", result.UnmatchedAgencyCodes);
        var saved = await ctx.AgencyBookings.SingleAsync();
        Assert.Null(saved.BuyingAgentId);
        Assert.False(saved.IsAgencyMatched);
        Assert.Equal("7391", saved.BuyingAgentCode); // vẫn giữ mã nguồn để đối soát
    }

    [Fact]
    public async Task Agency_found_links_internal_id()
    {
        using var ctx = CreateContext();
        var agency = new Agency
        {
            Code = "7391",
            Name = "Oneinventory_API_Klook",
            Source = "OneInventory",
            CreatedAtUtc = DateTime.UtcNow,
        };
        ctx.Agencies.Add(agency);
        await ctx.SaveChangesAsync();

        var service = NewService(Success(Line("111", buyingCode: "7391")), ctx);
        var result = await service.SyncAsync(BusinessDate, null, CancellationToken.None);

        Assert.Equal(0, result.UnmatchedAgency);
        var saved = await ctx.AgencyBookings.SingleAsync();
        Assert.Equal(agency.Id, saved.BuyingAgentId);
        Assert.True(saved.IsAgencyMatched);
    }

    [Fact]
    public async Task Rerun_same_day_is_idempotent_and_updates_on_change()
    {
        using var ctx = CreateContext();

        // Lần 1: thêm mới.
        var first = await NewService(Success(Line("111", amount: "100"), Line("222", amount: "200")), ctx)
            .SyncAsync(BusinessDate, null, CancellationToken.None);
        Assert.Equal(2, first.Inserted);

        // Lần 2: dữ liệu không đổi → bỏ qua, không tạo trùng.
        var second = await NewService(Success(Line("111", amount: "100"), Line("222", amount: "200")), ctx)
            .SyncAsync(BusinessDate, null, CancellationToken.None);
        Assert.Equal(0, second.Inserted);
        Assert.Equal(2, second.Skipped);
        Assert.Equal(2, await ctx.AgencyBookings.CountAsync());

        // Lần 3: 1 dòng đổi số tiền → cập nhật đúng 1.
        var third = await NewService(Success(Line("111", amount: "999"), Line("222", amount: "200")), ctx)
            .SyncAsync(BusinessDate, null, CancellationToken.None);
        Assert.Equal(1, third.Updated);
        Assert.Equal(1, third.Skipped);
        Assert.Equal(2, await ctx.AgencyBookings.CountAsync());
        var changed = await ctx.AgencyBookings.SingleAsync(x => x.SourceTransactionId == "111");
        Assert.Equal(999, changed.SalesAmount);
    }

    // ── Hạ tầng test ──

    private PpvReconDbContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        _connections.Add(connection);

        var options = new DbContextOptionsBuilder<PpvReconDbContext>()
            .UseSqlite(connection)
            .Options;
        var ctx = new PpvReconDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static AgencyBookingSyncService NewService(
        OneInventoryBookingResult apiResult, PpvReconDbContext ctx, string parentCode = ParentCode)
        => new(new FakeBookingApiClient(apiResult), new FakeConnectionSettings(parentCode), ctx);

    private static OneInventoryBookingResult Success(params OneInventoryBookingLine[] lines)
        => new() { IsSuccess = true, Lines = lines };

    private static OneInventoryBookingResult Failure(string code, string message)
        => new() { IsSuccess = false, ErrorCode = code, ErrorMessage = message };

    private static OneInventoryBookingLine Line(
        string id,
        string parent = ParentCode,
        string buyingCode = "7391",
        string amount = "423566",
        string booking = "67537839076")
        => new()
        {
            ID = id,
            MaDaiLyMuaCapTren = parent,
            TenDaiLyMuaCapTren = "Đại Lý ezCloud Mua_PL",
            MaDaiLyMua = buyingCode,
            TenDaiLyMua = "Oneinventory_API_Klook",
            MaDatVe = booking,
            NgayDat = "2026-07-21",
            SoLuongVe = "2",
            DonGia = "211783",
            TienBan = amount,
            TienVon = "415000",
            TamTinh = amount,
            GiamGia = "0",
            MaDaiLyBan = "5651",
            TenDaiLyBan = "KVC Oneinventory-Dragon",
            MaKhuVuiChoi = "11438",
            TenKhuVuiChoi = "Khu Du Lịch Quốc Tế Đồi Rồng",
            MaLoaiVe = "73b237b5-b057-4856-b52d-1e2b715c51bb",
            TenLoaiVe = "Người lớn (Trên 1,4m)",
            TenNhomLoaiVe = "1. Công Viên Nước",
        };

    public void Dispose()
    {
        foreach (var connection in _connections)
        {
            connection.Dispose();
        }
    }

    /// <summary>Client giả trả về kết quả cố định để kiểm thử service không phụ thuộc mạng.</summary>
    private sealed class FakeBookingApiClient(OneInventoryBookingResult result) : IOneInventoryBookingApiClient
    {
        public Task<OneInventoryBookingResult> FetchBookingsAsync(DateOnly businessDate, CancellationToken cancellationToken)
            => Task.FromResult(result);
    }

    /// <summary>Cấu hình giả: chỉ cần ParentAgencyCode; các phương thức khác không dùng trong test.</summary>
    private sealed class FakeConnectionSettings(string parentAgencyCode) : IConnectionSettingsService
    {
        public Task<OneInventoryApiOptions> GetOneInventoryAsync(CancellationToken cancellationToken)
            => Task.FromResult(new OneInventoryApiOptions { ParentAgencyCode = parentAgencyCode });

        public Task<ArApiOptions> GetArAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<BankStatementImportOptions> GetBankStatementAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<ParkBalanceApiOptions> GetParkBalanceAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<JobScheduleSettings> GetJobScheduleAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<ConnectionSettingsDto> GetAllAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task SaveAsync(ConnectionSettingsDto dto, int? userId, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<ConnectionTestResult> TestMailAsync(MailConnectionDto dto, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<ConnectionTestResult> TestParkBalanceAsync(ParkBalanceConnectionDto dto, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<ConnectionTestResult> TestOneInventoryAsync(OneInventoryConnectionDto dto, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}
