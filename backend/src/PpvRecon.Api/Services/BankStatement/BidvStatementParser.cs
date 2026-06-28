using System.Globalization;
using System.Text.RegularExpressions;

namespace PpvRecon.Api.Services.BankStatement;

/// <summary>Một giao dịch sao kê BIDV sau khi parse từ text PDF.</summary>
public sealed class ParsedTransaction
{
    public int STT { get; set; }
    public string NgayGD { get; set; } = "";
    public string NgayHL { get; set; } = "";
    public string MaGD { get; set; } = "";
    public decimal PhatSinhNo { get; set; }
    public decimal PhatSinhCo { get; set; }
    public decimal SoDu { get; set; }
    public string SoCT { get; set; } = "";
    public string MaGDV { get; set; } = "";
    public string MaCN { get; set; } = "";
    public string DienGiai { get; set; } = "";
}

/// <summary>
/// Parse text sao kê BIDV thành từng giao dịch. Port từ console GetPDFFromEmail.
///
/// Lưu ý: text do PdfPig trích ra bị DÍNH LIỀN, không có khoảng trắng giữa các cột.
/// Cấu trúc cột: STT | Ngày GD | (giờ) | Ngày HL | Mã GD | Phát sinh nợ |
///   Phát sinh có | Số dư | Số chứng từ | Mã GDV | Mã CN | Diễn giải
/// </summary>
public static class BidvStatementParser
{
    private static readonly Regex _recordRegex = new(
        @"(?<ngayGD>\d{2}/\d{2}/\d{4})(?<gio>\d{2}:\d{2}:\d{2})?" +
        @"(?<ngayHL>\d{2}/\d{2}/\d{4})" +
        @"(?<maGD>[A-Z]{2,8})" +
        @"(?<no>-?[\d,]+\.\d{2})" +
        @"(?<co>-?[\d,]+\.\d{2})" +
        @"(?<sodu>-?[\d,]+\.\d{2})" +
        @"(?<soct>\d{6})" +
        @"(?<magdv>[0-9A-Z]*[A-Z][0-9A-Z]*)" +
        @"(?<macn>\d{6})",
        RegexOptions.Compiled);

    private static readonly Regex _pageNumberRegex = new(@"Trang\s*\d+/\d+", RegexOptions.Compiled);
    private static readonly Regex _footerRegex = new(@"Chứng từ này được in.*?\d{2}:\d{2}:\d{2}", RegexOptions.Compiled);
    private static readonly Regex _columnHeaderRegex = new(@"STT\(No\).*?Diễn giải\(Txn\. Description\)", RegexOptions.Compiled);

    private static decimal ToNumber(string? v)
    {
        if (string.IsNullOrEmpty(v)) return 0m;
        var s = Regex.Replace(v, @"[^\d.,-]", "").Replace(",", "");
        return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : 0m;
    }

    public static List<ParsedTransaction> Parse(string raw)
    {
        var text = (raw ?? "")
            .Replace(' ', ' ')   // nbsp -> space
            .Replace("\r", "")
            .Replace("\n", "");

        text = _footerRegex.Replace(text, " ");
        text = _columnHeaderRegex.Replace(text, " ");
        text = _pageNumberRegex.Replace(text, " ");

        var matches = _recordRegex.Matches(text);

        var result = new List<ParsedTransaction>();
        for (int i = 0; i < matches.Count; i++)
        {
            var m = matches[i];

            int descStart = m.Index + m.Length;
            int descEnd = (i + 1 < matches.Count) ? matches[i + 1].Index : text.Length;
            var dienGiai = text.Substring(descStart, descEnd - descStart);
            dienGiai = Regex.Replace(dienGiai, @"\s+", " ").Trim();

            var gio = m.Groups["gio"].Value;
            var ngayGD = string.IsNullOrEmpty(gio)
                ? m.Groups["ngayGD"].Value
                : $"{m.Groups["ngayGD"].Value} {gio}";

            result.Add(new ParsedTransaction
            {
                STT = i + 1,
                NgayGD = ngayGD,
                NgayHL = m.Groups["ngayHL"].Value,
                MaGD = m.Groups["maGD"].Value,
                PhatSinhNo = ToNumber(m.Groups["no"].Value),
                PhatSinhCo = ToNumber(m.Groups["co"].Value),
                SoDu = ToNumber(m.Groups["sodu"].Value),
                SoCT = m.Groups["soct"].Value,
                MaGDV = m.Groups["magdv"].Value,
                MaCN = m.Groups["macn"].Value,
                DienGiai = dienGiai,
            });
        }

        return result;
    }
}
