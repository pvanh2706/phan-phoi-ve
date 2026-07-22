using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PpvRecon.Api.Services;
using PpvRecon.Infrastructure.Persistence;
using Xunit;

namespace PpvRecon.Tests;

/// <summary>
/// Kiểm thử luồng đồng bộ "Giao dịch của các đại lý trên AR" (§18): đọc Excel base64, bỏ 3 dòng đầu,
/// lọc "Thanh toán tiền cho booking", trích bookingId, mapping cột, chuẩn hóa ngày/tiền, chống trùng.
/// Dựng file .xlsx trong bộ nhớ bằng ClosedXML để không phụ thuộc API AR thật.
/// </summary>
public sealed class ArTransactionSyncServiceTests : IDisposable
{
    private static readonly DateOnly BusinessDate = new(2026, 7, 22);
    private readonly List<SqliteConnection> _connections = new();

    // ── Kịch bản ──

    [Fact]
    public async Task Valid_excel_imports_booking_payment_rows()
    {
        using var ctx = CreateContext();
        var excel = BuildWorkbook(ws =>
        {
            WriteTitles(ws);
            WriteRow(ws, 4, "AG001", "Đại lý A", "AR001", "22/07/2026 14:30:20", -500000.0, "Thanh toán tiền cho booking 123456");
        });

        var result = await Sync(ctx, excel);

        Assert.Equal(1, result.ValidBookingTransactions);
        Assert.Equal(1, result.Inserted);
        Assert.Equal(1, await ctx.AgencyArTransactions.CountAsync());
    }

    [Fact]
    public async Task Skips_first_three_rows()
    {
        using var ctx = CreateContext();
        var excel = BuildWorkbook(ws =>
        {
            // Đặt một dòng "hợp lệ" ngay tại dòng 3 (vùng tiêu đề) — phải bị bỏ qua.
            WriteRow(ws, 3, "AG000", "Tiêu đề", "AR000", "22/07/2026", 111000.0, "Thanh toán tiền cho booking 99999");
            WriteRow(ws, 4, "AG001", "Đại lý A", "AR001", "22/07/2026", 500000.0, "Thanh toán tiền cho booking 123456");
        });

        var result = await Sync(ctx, excel);

        Assert.Equal(1, result.ValidBookingTransactions);
        var saved = await ctx.AgencyArTransactions.SingleAsync();
        Assert.Equal("123456", saved.BookingId); // dòng 3 (99999) bị bỏ
    }

    [Fact]
    public async Task Only_booking_payment_rows_are_kept()
    {
        using var ctx = CreateContext();
        var excel = BuildWorkbook(ws =>
        {
            WriteTitles(ws);
            WriteRow(ws, 4, "AG1", "A", "AR1", "22/07/2026", 100000.0, "Thanh toán tiền cho booking 111");
            WriteRow(ws, 5, "AG2", "B", "AR2", "22/07/2026", 200000.0, "Nạp tiền từ phiếu chi");
            WriteRow(ws, 6, "AG3", "C", "AR3", "22/07/2026", 300000.0, "Hoàn tiền");
            WriteRow(ws, 7, "AG4", "D", "AR4", "22/07/2026", 400000.0, "Điều chỉnh số dư");
        });

        var result = await Sync(ctx, excel);

        Assert.Equal(1, result.ValidBookingTransactions);
        Assert.Equal(3, result.SkippedNonBookingRows);
        Assert.Equal(1, await ctx.AgencyArTransactions.CountAsync());
    }

    [Fact]
    public async Task Filter_is_case_insensitive()
    {
        using var ctx = CreateContext();
        var excel = OneRow("THANH TOÁN TIỀN CHO BOOKING 123456", "500000");
        var result = await Sync(ctx, excel);
        Assert.Equal(1, result.ValidBookingTransactions);
        Assert.Equal("123456", (await ctx.AgencyArTransactions.SingleAsync()).BookingId);
    }

    [Fact]
    public async Task Filter_allows_extra_whitespace()
    {
        using var ctx = CreateContext();
        var excel = OneRow("Thanh  toán   tiền cho  booking 123456", "500000");
        var result = await Sync(ctx, excel);
        Assert.Equal(1, result.ValidBookingTransactions);
    }

    [Fact]
    public async Task Extracts_booking_id()
    {
        using var ctx = CreateContext();
        var excel = OneRow("Thanh toán tiền cho booking 987654321", "500000");
        await Sync(ctx, excel);
        Assert.Equal("987654321", (await ctx.AgencyArTransactions.SingleAsync()).BookingId);
    }

    [Fact]
    public async Task Skips_row_without_booking_id()
    {
        using var ctx = CreateContext();
        var excel = OneRow("Thanh toán tiền cho booking ABC", "500000");
        var result = await Sync(ctx, excel);
        Assert.Equal(0, result.ValidBookingTransactions);
        Assert.Equal(1, result.ErrorRows);
        Assert.Equal(0, await ctx.AgencyArTransactions.CountAsync());
    }

    [Fact]
    public async Task Maps_all_columns_correctly()
    {
        using var ctx = CreateContext();
        var excel = BuildWorkbook(ws =>
        {
            WriteTitles(ws);
            WriteRow(ws, 4, "AG001", "Đại lý An", "AR777", "22/07/2026 09:15:00", 750000.0, "Thanh toán tiền cho booking 246810");
        });

        await Sync(ctx, excel);
        var saved = await ctx.AgencyArTransactions.SingleAsync();

        Assert.Equal("AG001", saved.TravelAgentCode);          // __EMPTY (A)
        Assert.Equal("Đại lý An", saved.TravelAgentName);       // __EMPTY_1 (B)
        Assert.Equal("AR777", saved.ReceivableAccountCode);     // __EMPTY_2 (C)
        Assert.Equal(750000, saved.Amount);                     // __EMPTY_4 (E)
        Assert.Equal("246810", saved.BookingId);                // từ __EMPTY_7 (H)
        Assert.Equal("Thanh toán tiền cho booking 246810", saved.Description);
    }

    [Fact]
    public async Task Negative_amount_stored_as_positive()
    {
        using var ctx = CreateContext();
        var excel = OneRow("Thanh toán tiền cho booking 123456", amount: -500000.0);
        await Sync(ctx, excel);
        Assert.Equal(500000, (await ctx.AgencyArTransactions.SingleAsync()).Amount);
    }

    [Fact]
    public async Task Parses_excel_serial_date()
    {
        using var ctx = CreateContext();
        // Serial number cho 22/07/2026 (không kèm giờ).
        var serial = new DateTime(2026, 7, 22).ToOADate();
        var excel = BuildWorkbook(ws =>
        {
            WriteTitles(ws);
            WriteRow(ws, 4, "AG1", "A", "AR1", serial, 500000.0, "Thanh toán tiền cho booking 123456");
        });

        await Sync(ctx, excel);
        var saved = await ctx.AgencyArTransactions.SingleAsync();
        Assert.Equal(2026, saved.TransactionDate.Year);
        Assert.Equal(7, saved.TransactionDate.Month);
        Assert.Equal(22, saved.TransactionDate.Day);
    }

    [Fact]
    public async Task Parses_ddmmyyyy_with_time()
    {
        using var ctx = CreateContext();
        var excel = BuildWorkbook(ws =>
        {
            WriteTitles(ws);
            WriteRow(ws, 4, "AG1", "A", "AR1", "22/07/2026 14:30:20", 500000.0, "Thanh toán tiền cho booking 123456");
        });

        await Sync(ctx, excel);
        var saved = await ctx.AgencyArTransactions.SingleAsync();
        Assert.Equal(new DateOnly(2026, 7, 22), saved.BusinessDate);
        Assert.Equal(14, saved.TransactionDate.Hour);
        Assert.Equal(30, saved.TransactionDate.Minute);
        Assert.Equal(20, saved.TransactionDate.Second);
    }

    [Fact]
    public async Task Does_not_swap_day_and_month()
    {
        using var ctx = CreateContext();
        // 13/02/2026: 13 không thể là tháng → chứng minh đọc theo DD/MM.
        var excel = BuildWorkbook(ws =>
        {
            WriteTitles(ws);
            WriteRow(ws, 4, "AG1", "A", "AR1", "13/02/2026", 500000.0, "Thanh toán tiền cho booking 123456");
        });

        await Sync(ctx, excel);
        var saved = await ctx.AgencyArTransactions.SingleAsync();
        Assert.Equal(13, saved.TransactionDate.Day);
        Assert.Equal(2, saved.TransactionDate.Month);
    }

    [Fact]
    public async Task Stores_wall_clock_without_shift_and_presents_as_vn_offset()
    {
        using var ctx = CreateContext();
        var excel = OneRow("Thanh toán tiền cho booking 123456", "500000", date: "22/07/2026 14:30:20");
        await Sync(ctx, excel);
        var saved = await ctx.AgencyArTransactions.SingleAsync();

        // Lưu đúng giờ tường VN, không bị cộng/trừ 7h (§10).
        Assert.Equal(14, saved.TransactionDate.Hour);
        Assert.Equal(DateTimeKind.Unspecified, saved.TransactionDate.Kind);

        // API gắn offset +07:00 mà không đổi giờ tường.
        var presented = new DateTimeOffset(DateTime.SpecifyKind(saved.TransactionDate, DateTimeKind.Unspecified), TimeSpan.FromHours(7));
        Assert.Equal(TimeSpan.FromHours(7), presented.Offset);
        Assert.Equal(14, presented.Hour);
    }

    [Fact]
    public async Task Rerun_same_day_is_idempotent()
    {
        using var ctx = CreateContext();

        var first = await Sync(ctx, OneRow("Thanh toán tiền cho booking 123456", "500000"));
        Assert.Equal(1, first.Inserted);

        var second = await Sync(ctx, OneRow("Thanh toán tiền cho booking 123456", "500000"));
        Assert.Equal(0, second.Inserted);
        Assert.Equal(1, second.Unchanged);
        Assert.Equal(1, await ctx.AgencyArTransactions.CountAsync());
    }

    [Fact]
    public async Task One_bad_row_does_not_stop_the_rest()
    {
        using var ctx = CreateContext();
        var excel = BuildWorkbook(ws =>
        {
            WriteTitles(ws);
            WriteRow(ws, 4, "AG1", "A", "AR1", "not-a-date", 500000.0, "Thanh toán tiền cho booking 111");
            WriteRow(ws, 5, "AG2", "B", "AR2", "22/07/2026", 600000.0, "Thanh toán tiền cho booking 222");
            WriteRow(ws, 6, "AG3", "C", "AR3", "22/07/2026", 700000.0, "Thanh toán tiền cho booking 333");
        });

        var result = await Sync(ctx, excel);

        Assert.Equal(1, result.ErrorRows);
        Assert.Equal(2, result.ValidBookingTransactions);
        Assert.Equal(2, await ctx.AgencyArTransactions.CountAsync());
    }

    [Fact]
    public async Task List_query_orders_by_transaction_date_desc()
    {
        using var ctx = CreateContext();
        var excel = BuildWorkbook(ws =>
        {
            WriteTitles(ws);
            WriteRow(ws, 4, "AG1", "A", "AR1", "20/07/2026", 100000.0, "Thanh toán tiền cho booking 111");
            WriteRow(ws, 5, "AG2", "B", "AR2", "22/07/2026", 200000.0, "Thanh toán tiền cho booking 222");
            WriteRow(ws, 6, "AG3", "C", "AR3", "21/07/2026", 300000.0, "Thanh toán tiền cho booking 333");
        });
        await Sync(ctx, excel);

        var ordered = await ctx.AgencyArTransactions
            .OrderByDescending(x => x.TransactionDate)
            .Select(x => x.BookingId)
            .ToListAsync();

        Assert.Equal(new[] { "222", "333", "111" }, ordered);
    }

    [Fact]
    public async Task Login_or_api_failure_throws()
    {
        using var ctx = CreateContext();
        var service = new ArTransactionSyncService(
            new FakeArApiClient(ArTransactionApiResultFailure("LoginFailed", "Đăng nhập AR thất bại.")),
            ctx,
            NullLogger<ArTransactionSyncService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SyncAsync(BusinessDate, null, CancellationToken.None));
        Assert.Equal(0, await ctx.AgencyArTransactions.CountAsync());
    }

    [Fact]
    public async Task Corrupt_excel_throws()
    {
        using var ctx = CreateContext();
        var service = new ArTransactionSyncService(
            new FakeArApiClient(ArTransactionApiResultSuccess(new byte[] { 1, 2, 3, 4 })),
            ctx,
            NullLogger<ArTransactionSyncService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SyncAsync(BusinessDate, null, CancellationToken.None));
    }

    // ── Hạ tầng test ──

    private Task<Application.Agencies.AgencyArTransactionSyncResult> Sync(PpvReconDbContext ctx, byte[] excel)
    {
        var service = new ArTransactionSyncService(
            new FakeArApiClient(ArTransactionApiResultSuccess(excel)),
            ctx,
            NullLogger<ArTransactionSyncService>.Instance);
        return service.SyncAsync(BusinessDate, null, CancellationToken.None);
    }

    private static byte[] OneRow(string description, object? amount = null, object? date = null)
        => BuildWorkbook(ws =>
        {
            WriteTitles(ws);
            WriteRow(ws, 4, "AG1", "Đại lý A", "AR1", date ?? "22/07/2026", amount ?? 500000.0, description);
        });

    private static void WriteTitles(IXLWorksheet ws)
    {
        ws.Cell(1, 1).Value = "LỊCH SỬ BIẾN ĐỘNG SỐ DƯ";
        ws.Cell(2, 1).Value = "Từ ngày ... đến ngày ...";
        ws.Cell(3, 1).Value = "Mã ĐL";
    }

    private static void WriteRow(
        IXLWorksheet ws, int row, string? code, string? name, string? recv, object? date, object? amount, string? desc)
    {
        if (code is not null) ws.Cell(row, 1).Value = code;
        if (name is not null) ws.Cell(row, 2).Value = name;
        if (recv is not null) ws.Cell(row, 3).Value = recv;
        if (date is not null) SetCell(ws.Cell(row, 4), date);
        if (amount is not null) SetCell(ws.Cell(row, 5), amount);
        if (desc is not null) ws.Cell(row, 8).Value = desc;
    }

    private static void SetCell(IXLCell cell, object value)
    {
        switch (value)
        {
            case string s: cell.Value = s; break;
            case DateTime dt: cell.Value = dt; break;
            case double d: cell.Value = d; break;
            case int i: cell.Value = i; break;
            default: cell.Value = value.ToString(); break;
        }
    }

    private static byte[] BuildWorkbook(Action<IXLWorksheet> fill)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Sheet1");
        fill(ws);
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static ArTransactionApiResult ArTransactionApiResultSuccess(byte[] bytes)
        => new()
        {
            IsSuccess = true,
            FileContents = bytes,
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileDownloadName = "LichSuBienDongSoDu.xlsx",
        };

    private static ArTransactionApiResult ArTransactionApiResultFailure(string code, string message)
        => new() { IsSuccess = false, ErrorCode = code, ErrorMessage = message };

    private PpvReconDbContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        _connections.Add(connection);
        var options = new DbContextOptionsBuilder<PpvReconDbContext>().UseSqlite(connection).Options;
        var ctx = new PpvReconDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    public void Dispose()
    {
        foreach (var connection in _connections)
        {
            connection.Dispose();
        }
    }

    private sealed class FakeArApiClient(ArTransactionApiResult result) : IArTransactionApiClient
    {
        public Task<ArTransactionApiResult> FetchTransactionsAsync(DateOnly businessDate, CancellationToken cancellationToken)
            => Task.FromResult(result);
    }
}
