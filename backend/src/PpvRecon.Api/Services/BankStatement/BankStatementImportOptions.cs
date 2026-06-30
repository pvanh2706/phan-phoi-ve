namespace PpvRecon.Api.Services.BankStatement;

/// <summary>
/// Cấu hình đọc sao kê BIDV từ email (section "BankStatementImport" trong appsettings.json).
/// </summary>
public sealed class BankStatementImportOptions
{
    public string Host { get; set; } = "imap.gmail.com";
    public int Port { get; set; } = 993;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = "";

    /// <summary>Gmail: dùng App Password, KHÔNG dùng mật khẩu thường.</summary>
    public string Password { get; set; } = "";
    public string Mailbox { get; set; } = "INBOX";

    /// <summary>Lọc người gửi (mặc định BIDV).</summary>
    public string FromFilter { get; set; } = "insaoke@bidv.com.vn";

    /// <summary>Mật khẩu mở file PDF sao kê.</summary>
    public string PdfPassword { get; set; } = "";
}
