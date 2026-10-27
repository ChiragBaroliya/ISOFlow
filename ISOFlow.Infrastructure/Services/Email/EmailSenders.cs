using ISOFlow.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace ISOFlow.Infrastructure.Services.Email;

/// <summary>"Email" configuration section.</summary>
public sealed class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>Sender address shown to recipients, e.g. no-reply@yourcompany.com.</summary>
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "ISOFlow";

    public SmtpSettings Smtp { get; set; } = new();

    /// <summary>True when an SMTP server and sender address are configured.</summary>
    public bool IsSmtpConfigured => !string.IsNullOrWhiteSpace(Smtp.Host) && !string.IsNullOrWhiteSpace(FromAddress);
}

/// <summary>
/// SMTP server settings. Works with any SMTP provider (Office 365, Gmail, SendGrid's SMTP relay, a corporate relay).
/// <see cref="Password"/> is a secret: supply it via User Secrets or the <c>Email__Smtp__Password</c> environment variable.
/// </summary>
public sealed class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    /// <summary>StartTls (port 587), SslOnConnect (port 465), or None (local relays only).</summary>
    public string Security { get; set; } = "StartTls";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>Delivers email through an SMTP server using MailKit.</summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(EmailSettings settings)
    {
        _settings = settings;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        mime.To.Add(new MailboxAddress(message.ToName, message.ToAddress));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { HtmlBody = message.HtmlBody, TextBody = message.TextBody }.ToMessageBody();

        var security = Enum.TryParse<SecureSocketOptions>(_settings.Smtp.Security, ignoreCase: true, out var parsed)
            ? parsed
            : SecureSocketOptions.StartTls;

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Smtp.Host, _settings.Smtp.Port, security, cancellationToken);
        if (!string.IsNullOrEmpty(_settings.Smtp.Username))
            await client.AuthenticateAsync(_settings.Smtp.Username, _settings.Smtp.Password, cancellationToken);
        await client.SendAsync(mime, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }
}

/// <summary>
/// Development-only sender used when no SMTP server is configured: writes the email (including any links) to the
/// application log instead of delivering it, so flows like password reset can be exercised locally.
/// Never registered outside the Development environment.
/// </summary>
public sealed class DevelopmentLogEmailSender : IEmailSender
{
    private readonly ILogger<DevelopmentLogEmailSender> _logger;

    public DevelopmentLogEmailSender(ILogger<DevelopmentLogEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "[DEV EMAIL - not delivered, no SMTP configured] To: {To} | Subject: {Subject}{NewLine}{Body}",
            message.ToAddress, message.Subject, Environment.NewLine, message.TextBody);
        return Task.CompletedTask;
    }
}

/// <summary>Registered outside Development when SMTP is not configured: every send fails loudly.</summary>
public sealed class UnconfiguredEmailSender : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(
            "Email is not configured. Set Email:FromAddress and Email:Smtp:Host (and credentials) to enable delivery.");
}
