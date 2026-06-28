using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace PpvRecon.Api.Services.BankStatement;

/// <summary>Một email đã tải về cùng các attachment dạng byte[].</summary>
public sealed class FetchedEmail
{
    public string Subject { get; init; } = "";
    public string From { get; init; } = "";
    public string Date { get; init; } = "";
    public List<(string FileName, byte[] Data)> Attachments { get; init; } = new();
}

public interface IImapEmailReader
{
    /// <summary>Lấy N email gần nhất từ người gửi đã cấu hình (không đánh dấu đã đọc).</summary>
    Task<List<FetchedEmail>> FetchRecentAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Đọc email qua IMAP, lấy N mail gần nhất từ BIDV. Port từ console GetPDFFromEmail.
/// </summary>
public sealed class ImapEmailReader(IOptions<BankStatementImportOptions> options) : IImapEmailReader
{
    private readonly BankStatementImportOptions _opt = options.Value;

    public async Task<List<FetchedEmail>> FetchRecentAsync(CancellationToken cancellationToken)
    {
        var results = new List<FetchedEmail>();

        using var client = new ImapClient();
        var socketOptions = _opt.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
        await client.ConnectAsync(_opt.Host, _opt.Port, socketOptions, cancellationToken);
        await client.AuthenticateAsync(_opt.Username, _opt.Password, cancellationToken);

        var folder = string.Equals(_opt.Mailbox, "INBOX", StringComparison.OrdinalIgnoreCase)
            ? client.Inbox
            : await client.GetFolderAsync(_opt.Mailbox, cancellationToken);
        await folder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

        SearchQuery query = SearchQuery.All;
        if (!string.IsNullOrWhiteSpace(_opt.FromFilter))
            query = query.And(SearchQuery.FromContains(_opt.FromFilter));

        var uids = (await folder.SearchAsync(query, cancellationToken)).ToList();

        // Giữ N mail mới nhất (UID lớn nhất = mail đến sau cùng).
        if (_opt.RecentCount > 0 && uids.Count > _opt.RecentCount)
            uids = uids.OrderBy(u => u.Id).TakeLast(_opt.RecentCount).ToList();

        foreach (var uid in uids)
        {
            var message = await folder.GetMessageAsync(uid, cancellationToken);

            var fetched = new FetchedEmail
            {
                Subject = message.Subject ?? "(no subject)",
                From = message.From?.ToString() ?? "",
                Date = message.Date.ToString("o"),
            };

            foreach (var attachment in message.Attachments)
            {
                if (attachment is MimeKit.MimePart { Content: not null } part)
                {
                    using var ms = new MemoryStream();
                    await part.Content.DecodeToAsync(ms, cancellationToken);
                    var name = part.FileName ?? "unnamed";
                    fetched.Attachments.Add((name, ms.ToArray()));
                }
            }

            results.Add(fetched);
        }

        await client.DisconnectAsync(true, cancellationToken);
        return results;
    }
}
