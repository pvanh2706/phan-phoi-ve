using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Domain.Entities.Parks;
using PpvRecon.Domain.Entities.Summaries;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Services.BankStatement;

/// <summary>Kết quả một lần đồng bộ sao kê từ email.</summary>
public sealed class BankStatementSyncResult
{
    public int MailsProcessed { get; set; }
    public int TransactionsParsed { get; set; }
    public int Imported { get; set; }
    public int SkippedUnmatched { get; set; }
    public int OverwrittenDates { get; set; }

    /// <summary>Các số TKThe không khớp Park nào (để cảnh báo người dùng).</summary>
    public List<string> UnmatchedAccounts { get; set; } = new();
}

public interface IBankStatementSyncService
{
    Task<BankStatementSyncResult> SyncAsync(int? currentUserId, CancellationToken cancellationToken);
}

/// <summary>
/// Lấy sao kê BIDV từ email → trích PDF → parse → map Park qua TKThe →
/// ghi đè theo ngày vào bảng BankTransactionDetails.
/// </summary>
public sealed class BankStatementSyncService(
    IImapEmailReader emailReader,
    Microsoft.Extensions.Options.IOptions<BankStatementImportOptions> options,
    PpvReconDbContext dbContext) : IBankStatementSyncService
{
    private readonly BankStatementImportOptions optionsValue = options.Value;

    // Lấy số sau "TKThe :" (bao trùm cả dạng "HBK-TKThe :"). Giá trị có thể chứa chữ + số.
    private static readonly Regex _tkTheRegex = new(
        @"TKThe\s*:\s*(?<tk>[^,\s]+)", RegexOptions.Compiled);

    public async Task<BankStatementSyncResult> SyncAsync(int? currentUserId, CancellationToken cancellationToken)
    {
        var result = new BankStatementSyncResult();

        var emails = await emailReader.FetchRecentAsync(cancellationToken);

        // 1) Parse toàn bộ giao dịch từ các PDF đính kèm.
        var parsed = new List<ParsedTransaction>();
        foreach (var email in emails)
        {
            var pdf = email.Attachments.FirstOrDefault(a =>
                a.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
            if (pdf.Data is null || pdf.Data.Length == 0) continue;

            string pdfText;
            try
            {
                pdfText = PdfExtractor.ExtractText(pdf.Data, optionsValue.PdfPassword);
            }
            catch
            {
                // PDF lỗi/mật khẩu sai -> bỏ qua email này, xử lý các email còn lại.
                continue;
            }

            parsed.AddRange(BidvStatementParser.Parse(pdfText));
            result.MailsProcessed++;
        }

        result.TransactionsParsed = parsed.Count;
        if (parsed.Count == 0) return result;

        // 2) Map Park qua TKThe == Park.BankAccount.
        var parks = await dbContext.Parks
            .Where(p => !p.IsDeleted && p.BankAccount != null && p.BankAccount != "")
            .Select(p => new { p.Id, p.BankAccount, p.PaymentType })
            .ToListAsync(cancellationToken);

        var parkByAccount = parks
            .GroupBy(p => p.BankAccount!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var matched = new List<BankTransactionDetail>();
        var unmatched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var nowUtc = DateTime.UtcNow;

        foreach (var t in parsed)
        {
            var tkThe = _tkTheRegex.Match(t.DienGiai).Groups["tk"].Value.Trim();
            if (string.IsNullOrEmpty(tkThe) || !parkByAccount.TryGetValue(tkThe, out var park))
            {
                if (!string.IsNullOrEmpty(tkThe)) unmatched.Add(tkThe);
                result.SkippedUnmatched++;
                continue;
            }

            var businessDate = ParseDateOnly(t.NgayHL) ?? ParseDateOnly(t.NgayGD);
            if (businessDate is null)
            {
                result.SkippedUnmatched++;
                continue;
            }

            matched.Add(new BankTransactionDetail
            {
                BusinessDate = businessDate.Value,
                TransactionAtUtc = ParseDateTime(t.NgayGD) ?? businessDate.Value.ToDateTime(TimeOnly.MinValue),
                PaymentType = park.PaymentType,
                DebitAmount = ToLong(t.SoDu),         // theo yêu cầu: cột "Ghi nợ" hiển thị Số dư
                CreditAmount = ToLong(t.PhatSinhCo),
                Content = t.DienGiai,
                BankAccount = tkThe,
                ParkId = park.Id,
                SourceType = SourceType.Api,
                CreatedAtUtc = nowUtc,
                CreatedByUserId = currentUserId,
            });
        }

        result.UnmatchedAccounts = unmatched.ToList();
        if (matched.Count == 0) return result;

        // 3) Ghi đè theo ngày: với mỗi ngày có dữ liệu mới, xóa GD nguồn API của ngày đó rồi nạp lại.
        var datesToOverwrite = matched.Select(m => m.BusinessDate).Distinct().ToList();
        result.OverwrittenDates = datesToOverwrite.Count;

        var existing = await dbContext.BankTransactionDetails
            .Where(x => x.SourceType == SourceType.Api && datesToOverwrite.Contains(x.BusinessDate))
            .ToListAsync(cancellationToken);
        dbContext.BankTransactionDetails.RemoveRange(existing);

        await dbContext.BankTransactionDetails.AddRangeAsync(matched, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        result.Imported = matched.Count;
        return result;
    }

    private static long ToLong(decimal value) =>
        decimal.ToInt64(decimal.Round(value, 0, MidpointRounding.AwayFromZero));

    private static DateOnly? ParseDateOnly(string value)
    {
        var datePart = (value ?? "").Trim();
        var spaceIndex = datePart.IndexOf(' ');
        if (spaceIndex > 0) datePart = datePart[..spaceIndex];
        return DateOnly.TryParseExact(datePart, "dd/MM/yyyy", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var d) ? d : null;
    }

    private static DateTime? ParseDateTime(string value)
    {
        var s = (value ?? "").Trim();
        if (DateTime.TryParseExact(s, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var dt)) return dt;
        if (DateTime.TryParseExact(s, "dd/MM/yyyy", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var d)) return d;
        return null;
    }
}
