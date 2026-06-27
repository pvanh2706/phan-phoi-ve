using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace PpvRecon.Api.Services;

public interface IEmailSender
{
    Task<EmailSendResult> SendAsync(
        IReadOnlyCollection<string> recipients,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}

public sealed record EmailSendResult(bool Attempted, bool Sent, string? Message);

public sealed class SmtpEmailSender(
    IOptions<EmailOptions> options,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task<EmailSendResult> SendAsync(
        IReadOnlyCollection<string> recipients,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        if (recipients.Count == 0)
        {
            return new EmailSendResult(false, false, "Không có người nhận email.");
        }

        if (string.IsNullOrWhiteSpace(_options.SmtpHost) || string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            logger.LogWarning("SMTP is not configured. Skip sending email subject {Subject}.", subject);
            return new EmailSendResult(false, false, "SMTP chưa cấu hình, đã bỏ qua gửi email.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = false,
        };

        foreach (var recipient in recipients.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            message.To.Add(recipient);
        }

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
        };

        if (!string.IsNullOrWhiteSpace(_options.SmtpUsername))
        {
            client.Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword);
        }

        await client.SendMailAsync(message, cancellationToken);
        return new EmailSendResult(true, true, "Đã gửi email.");
    }
}
