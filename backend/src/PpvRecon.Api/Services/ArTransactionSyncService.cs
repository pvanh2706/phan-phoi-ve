using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Agencies;
using PpvRecon.Domain.Entities.Agencies;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Services;

public interface IArTransactionSyncService
{
    /// <summary>
    /// Lấy file Excel giao dịch AR cho đúng 1 ngày, đọc từ memory, lọc "Thanh toán tiền cho booking",
    /// trích bookingId, chuẩn hóa ngày/tiền và upsert chống trùng theo DedupHash (§13).
    /// </summary>
    Task<AgencyArTransactionSyncResult> SyncAsync(DateOnly businessDate, int? currentUserId, CancellationToken cancellationToken);
}

/// <summary>
/// Đồng bộ "Giao dịch của các đại lý trên AR":
/// 1) Gọi AR lấy file Excel base64 (tái dùng <see cref="IArTransactionApiClient"/>).
/// 2) Đọc Excel từ memory stream; bỏ 3 dòng tiêu đề/báo cáo đầu file (§6).
/// 3) Chỉ giữ dòng mô tả (__EMPTY_7) bắt đầu bằng "Thanh toán tiền cho booking" (§7).
/// 4) Trích bookingId (dãy số sau "booking"); chuẩn hóa ngày (§10) và tiền dương (§11).
/// 5) Upsert theo DedupHash để idempotent khi chạy lại (§13).
/// Mapping cột (đã xác nhận): A=Mã ĐL, B=Tên ĐL, C=Mã TK công nợ, D=Ngày, E=Số tiền, H=Mô tả.
/// </summary>
public sealed partial class ArTransactionSyncService(
    IArTransactionApiClient apiClient,
    PpvReconDbContext dbContext,
    ILogger<ArTransactionSyncService> logger) : IArTransactionSyncService
{
    // Bỏ 3 dòng đầu file (§6).
    private const int HeaderRowsToSkip = 3;
    private const int MaxWarnings = 20;

    // §15: serialize các lần đồng bộ cùng một ngày (scheduler + chạy tay) để tránh chạy chồng gây trùng.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<DateOnly, SemaphoreSlim> DayLocks = new();

    // Cột theo mapping đã xác nhận (ClosedXML dùng chỉ số cột 1-based: A=1..H=8).
    private const int ColTravelAgentCode = 1;      // A  __EMPTY
    private const int ColTravelAgentName = 2;      // B  __EMPTY_1
    private const int ColReceivableAccount = 3;    // C  __EMPTY_2
    private const int ColTransactionDate = 4;      // D  __EMPTY_3
    private const int ColAmount = 5;               // E  __EMPTY_4
    private const int ColDescription = 8;          // H  __EMPTY_7

    [GeneratedRegex(@"^Thanh\s*toán\s*tiền\s*cho\s*booking", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BookingPaymentRegex();

    [GeneratedRegex(@"booking\s+(\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BookingIdRegex();

    public async Task<AgencyArTransactionSyncResult> SyncAsync(
        DateOnly businessDate, int? currentUserId, CancellationToken cancellationToken)
    {
        var gate = DayLocks.GetOrAdd(businessDate, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            return await RunSyncAsync(businessDate, currentUserId, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<AgencyArTransactionSyncResult> RunSyncAsync(
        DateOnly businessDate, int? currentUserId, CancellationToken cancellationToken)
    {
        var result = new AgencyArTransactionSyncResult { BusinessDate = businessDate };

        var apiResult = await apiClient.FetchTransactionsAsync(businessDate, cancellationToken);
        if (!apiResult.IsSuccess || apiResult.FileContents is null)
        {
            throw new InvalidOperationException(apiResult.ErrorMessage ?? "Không lấy được dữ liệu từ AR.");
        }

        // Đọc Excel trực tiếp từ memory (không ghi file tạm §5).
        var candidates = ParseWorkbook(apiResult.FileContents, result);
        if (candidates.Count == 0)
        {
            return result;
        }

        // §13: dedupe trong batch (cùng DedupHash → lấy dòng cuối) rồi nạp bản ghi đã tồn tại để upsert.
        var byHash = new Dictionary<string, ArTransactionCandidate>(StringComparer.Ordinal);
        foreach (var c in candidates)
        {
            byHash[c.DedupHash] = c;
        }

        var hashes = byHash.Keys.ToList();
        var existingByHash = new Dictionary<string, AgencyArTransaction>(StringComparer.Ordinal);
        foreach (var chunk in hashes.Chunk(500))
        {
            var rows = await dbContext.AgencyArTransactions
                .Where(x => chunk.Contains(x.DedupHash))
                .ToListAsync(cancellationToken);
            foreach (var row in rows)
            {
                existingByHash[row.DedupHash] = row;
            }
        }

        var nowUtc = DateTime.UtcNow;
        foreach (var c in byHash.Values)
        {
            if (existingByHash.TryGetValue(c.DedupHash, out var existing))
            {
                // Bản ghi đã có: chỉ cập nhật khi trường không thuộc khóa thay đổi (tên đại lý/mô tả).
                var changed = existing.TravelAgentName != c.TravelAgentName || existing.Description != c.Description;
                if (changed)
                {
                    existing.TravelAgentName = c.TravelAgentName;
                    existing.Description = c.Description;
                    existing.UpdatedAtUtc = nowUtc;
                    existing.UpdatedByUserId = currentUserId;
                    result.Updated++;
                }
                else
                {
                    result.Unchanged++;
                }
            }
            else
            {
                dbContext.AgencyArTransactions.Add(new AgencyArTransaction
                {
                    BookingId = c.BookingId,
                    TravelAgentName = c.TravelAgentName,
                    TravelAgentCode = c.TravelAgentCode,
                    ReceivableAccountCode = c.ReceivableAccountCode,
                    TransactionDate = c.TransactionDate,
                    BusinessDate = c.BusinessDate,
                    Amount = c.Amount,
                    Description = c.Description,
                    DedupHash = c.DedupHash,
                    SourceRowNumber = c.SourceRowNumber,
                    SourceType = SourceType.Api,
                    CreatedAtUtc = nowUtc,
                    CreatedByUserId = currentUserId,
                });
                result.Inserted++;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private List<ArTransactionCandidate> ParseWorkbook(
        byte[] fileContents, AgencyArTransactionSyncResult result)
    {
        var candidates = new List<ArTransactionCandidate>();

        using var stream = new MemoryStream(fileContents);
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(stream);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Không đọc được file Excel từ AR: {ex.Message}", ex);
        }

        using (workbook)
        {
            var ws = workbook.Worksheets.FirstOrDefault()
                ?? throw new InvalidOperationException("File Excel AR không có worksheet nào.");

            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
            result.TotalRows = lastRow;
            result.SkippedHeaderRows = Math.Min(HeaderRowsToSkip, lastRow);

            for (var rowNum = HeaderRowsToSkip + 1; rowNum <= lastRow; rowNum++)
            {
                var row = ws.Row(rowNum);
                var description = row.Cell(ColDescription).GetString().Trim();
                if (description.Length == 0)
                {
                    continue; // dòng không có nội dung tại __EMPTY_7 → bỏ qua (§6)
                }

                result.RowsWithDescription++;

                if (!BookingPaymentRegex().IsMatch(description))
                {
                    result.SkippedNonBookingRows++;
                    continue; // không phải "Thanh toán tiền cho booking" (§7)
                }

                var bookingMatch = BookingIdRegex().Match(description);
                if (!bookingMatch.Success)
                {
                    RecordError(result, rowNum, $"không trích được bookingId từ mô tả: \"{description}\"");
                    continue;
                }
                var bookingId = bookingMatch.Groups[1].Value;

                if (!TryParseVnDate(row.Cell(ColTransactionDate), out var transactionDate))
                {
                    RecordError(result, rowNum, $"ngày giao dịch không hợp lệ (mô tả: \"{description}\")");
                    continue;
                }

                if (!TryParseAmount(row.Cell(ColAmount), out var amount))
                {
                    RecordError(result, rowNum, $"số tiền không hợp lệ (mô tả: \"{description}\")");
                    continue;
                }

                var agentCode = TrimOrNull(row.Cell(ColTravelAgentCode).GetString());
                var agentName = TrimOrNull(row.Cell(ColTravelAgentName).GetString());
                var receivableCode = TrimOrNull(row.Cell(ColReceivableAccount).GetString());

                result.ValidBookingTransactions++;
                candidates.Add(new ArTransactionCandidate
                {
                    BookingId = bookingId,
                    TravelAgentName = agentName,
                    TravelAgentCode = agentCode,
                    ReceivableAccountCode = receivableCode,
                    TransactionDate = transactionDate,
                    BusinessDate = DateOnly.FromDateTime(transactionDate),
                    Amount = amount,
                    Description = description,
                    SourceRowNumber = rowNum,
                    DedupHash = ComputeDedupHash(bookingId, transactionDate, amount, agentCode, receivableCode),
                });
            }
        }

        return candidates;
    }

    private void RecordError(AgencyArTransactionSyncResult result, int rowNum, string reason)
    {
        result.ErrorRows++;
        logger.LogWarning("AR sync: bỏ qua dòng Excel {Row} — {Reason}.", rowNum, reason);
        if (result.Warnings.Count < MaxWarnings)
        {
            result.Warnings.Add($"Dòng {rowNum}: {reason}");
        }
    }

    /// <summary>
    /// Chuẩn hóa ngày (§10): hỗ trợ cell DateTime, serial number, và chuỗi DD/MM/YYYY[ HH:mm:ss]/ISO.
    /// Trả về giờ tường (VN) dạng DateTime Unspecified — không quy đổi UTC, không đảo ngày/tháng.
    /// </summary>
    private static bool TryParseVnDate(IXLCell cell, out DateTime value)
    {
        value = default;
        DateTime dt;

        if (cell.DataType == XLDataType.DateTime)
        {
            dt = cell.GetDateTime();
        }
        else if (cell.DataType == XLDataType.Number)
        {
            try { dt = DateTime.FromOADate(cell.GetDouble()); }
            catch { return false; }
        }
        else
        {
            var s = cell.GetString().Trim();
            if (s.Length == 0)
            {
                return false;
            }

            string[] formats =
            {
                "dd/MM/yyyy HH:mm:ss", "d/M/yyyy HH:mm:ss",
                "dd/MM/yyyy HH:mm", "d/M/yyyy HH:mm",
                "dd/MM/yyyy", "d/M/yyyy",
            };
            // Ưu tiên định dạng Việt Nam DD/MM/YYYY, không để tự đảo ngày/tháng (§10).
            if (!DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                // Chuỗi ISO có 'T' mới thử parse ISO (tránh nhầm MM/dd với chuỗi có dấu '/').
                if (!(s.Contains('T', StringComparison.Ordinal)
                    && DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)))
                {
                    return false;
                }
            }
        }

        value = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
        return true;
    }

    /// <summary>Chuẩn hóa tiền (§11): lấy trị tuyệt đối, kiểu long, không tự làm tròn phần lẻ.</summary>
    private static bool TryParseAmount(IXLCell cell, out long amount)
    {
        amount = 0;
        decimal value;

        if (cell.DataType == XLDataType.Number)
        {
            var raw = cell.GetDouble();
            var rounded = Math.Round(raw);
            if (Math.Abs(raw - rounded) > 1e-6)
            {
                return false; // có phần lẻ → coi là lỗi, không tự làm tròn
            }
            value = (decimal)rounded;
        }
        else
        {
            var s = cell.GetString().Trim().Replace(" ", "");
            if (s.Length == 0)
            {
                return false;
            }
            if (!decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value)
                && !decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowLeadingSign, new CultureInfo("vi-VN"), out value))
            {
                return false;
            }
            if (value != Math.Truncate(value))
            {
                return false;
            }
        }

        value = Math.Abs(value);
        if (value > long.MaxValue)
        {
            return false;
        }
        amount = (long)value;
        return true;
    }

    private static string ComputeDedupHash(
        string bookingId, DateTime transactionDate, long amount, string? agentCode, string? receivableCode)
    {
        var raw = string.Join('|',
            bookingId,
            transactionDate.ToString("O", CultureInfo.InvariantCulture),
            amount.ToString(CultureInfo.InvariantCulture),
            agentCode ?? string.Empty,
            receivableCode ?? string.Empty);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }

    private static string? TrimOrNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed class ArTransactionCandidate
    {
        public string BookingId { get; init; } = string.Empty;
        public string? TravelAgentName { get; init; }
        public string? TravelAgentCode { get; init; }
        public string? ReceivableAccountCode { get; init; }
        public DateTime TransactionDate { get; init; }
        public DateOnly BusinessDate { get; init; }
        public long Amount { get; init; }
        public string Description { get; init; } = string.Empty;
        public int SourceRowNumber { get; init; }
        public string DedupHash { get; init; } = string.Empty;
    }
}
