using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using PpvRecon.Api.Services.Settings;

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
    /// <summary>
    /// Lấy tất cả email trong ngày <paramref name="localDate"/> (giờ VN) từ người gửi đã cấu hình.
    /// Mở hộp thư ở chế độ chỉ đọc nên KHÔNG đánh dấu mail đã đọc.
    /// </summary>
    Task<List<FetchedEmail>> FetchForDateAsync(DateOnly localDate, CancellationToken cancellationToken);
}

/// <summary>
/// Đọc email qua IMAP, lấy các mail BIDV phát sinh trong một ngày. Port từ console GetPDFFromEmail.
/// </summary>
public sealed class ImapEmailReader(IConnectionSettingsService connectionSettings) : IImapEmailReader
{
    public async Task<List<FetchedEmail>> FetchForDateAsync(DateOnly localDate, CancellationToken cancellationToken)
    {
        var results = new List<FetchedEmail>();
        var timeZone = GetVietnamTimeZone();
        var opt = await connectionSettings.GetBankStatementAsync(cancellationToken);

        using var client = new ImapClient();
        var socketOptions = opt.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
        await client.ConnectAsync(opt.Host, opt.Port, socketOptions, cancellationToken);
        await client.AuthenticateAsync(opt.Username, opt.Password, cancellationToken);

        var folder = string.Equals(opt.Mailbox, "INBOX", StringComparison.OrdinalIgnoreCase)
            ? client.Inbox
            : await client.GetFolderAsync(opt.Mailbox, cancellationToken);
        await folder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

        // Lọc thô ở server theo ngày nhận (lùi 1 ngày để bao biên múi giờ), tinh chỉnh đúng ngày ở dưới.
        SearchQuery query = SearchQuery.DeliveredAfter(localDate.AddDays(-1).ToDateTime(TimeOnly.MinValue));
        if (!string.IsNullOrWhiteSpace(opt.FromFilter))
            query = query.And(SearchQuery.FromContains(opt.FromFilter));

        var uids = (await folder.SearchAsync(query, cancellationToken)).ToList();

        foreach (var uid in uids)
        {
            var message = await folder.GetMessageAsync(uid, cancellationToken);

            // Chỉ giữ mail có ngày gửi (quy về giờ VN) đúng bằng ngày đang quét.
            var localSentDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(message.Date, timeZone).DateTime);
            if (localSentDate != localDate) continue;

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

    private static TimeZoneInfo GetVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
        }
    }
}
