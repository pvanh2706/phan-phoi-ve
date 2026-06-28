using System.Text;
using UglyToad.PdfPig;

namespace PpvRecon.Api.Services.BankStatement;

/// <summary>
/// Trích text từ file PDF sao kê (hỗ trợ PDF có mật khẩu).
/// Port từ console GetPDFFromEmail.
/// </summary>
public static class PdfExtractor
{
    public static string ExtractText(byte[] pdfBytes, string? password)
    {
        var options = new ParsingOptions
        {
            Passwords = string.IsNullOrEmpty(password)
                ? new List<string>()
                : new List<string> { password },
        };

        using var document = PdfDocument.Open(pdfBytes, options);
        var sb = new StringBuilder();
        foreach (var page in document.GetPages())
        {
            sb.AppendLine(page.Text);
        }
        return sb.ToString();
    }
}
