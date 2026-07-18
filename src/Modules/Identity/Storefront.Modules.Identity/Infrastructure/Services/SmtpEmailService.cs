using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Infrastructure.Services;

/// <summary>
/// SMTP implementation of IEmailService via MailKit.
/// No-ops (with a warning) when Email:Host is not configured so local
/// development works without an SMTP server.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var host = _configuration["Email:Host"];

        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning("Email:Host is not configured — skipping email '{Subject}' to {To}", subject, to);
            return;
        }

        if (string.IsNullOrWhiteSpace(to))
            return;

        try
        {
            var port        = _configuration.GetValue("Email:Port", 587);
            var username    = _configuration["Email:Username"];
            var password    = _configuration["Email:Password"];
            var fromAddress = _configuration["Email:FromAddress"] ?? username ?? "noreply@localhost";
            var fromName    = _configuration["Email:FromName"] ?? "Storefront";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTlsWhenAvailable, ct);

            if (!string.IsNullOrWhiteSpace(username))
                await client.AuthenticateAsync(username, password, ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("Email '{Subject}' sent to {To}", subject, to);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Email is always a side effect — never crash the calling operation
            _logger.LogError(ex, "Failed to send email '{Subject}' to {To}", subject, to);
        }
    }
}
