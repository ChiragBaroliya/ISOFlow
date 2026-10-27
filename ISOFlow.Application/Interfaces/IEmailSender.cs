namespace ISOFlow.Application.Interfaces;

/// <summary>An outgoing email. <see cref="TextBody"/> is the plain-text alternative to <see cref="HtmlBody"/>.</summary>
public sealed record EmailMessage(string ToAddress, string ToName, string Subject, string HtmlBody, string TextBody);

/// <summary>Sends transactional email (password resets, notifications). Throws when delivery fails.</summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
