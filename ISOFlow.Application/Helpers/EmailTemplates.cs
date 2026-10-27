using System.Net;
using ISOFlow.Application.Interfaces;

namespace ISOFlow.Application.Helpers;

/// <summary>Builds the transactional emails ISOFlow sends.</summary>
public static class EmailTemplates
{
    public static EmailMessage PasswordReset(string toAddress, string toName, string resetUrl, TimeSpan lifetime)
    {
        var minutes = (int)lifetime.TotalMinutes;
        var name = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(toName) ? toAddress : toName);
        var url = WebUtility.HtmlEncode(resetUrl);

        var html = $"""
            <p>Hello {name},</p>
            <p>We received a request to reset your ISOFlow password. Use the link below to choose a new one.
               The link expires in {minutes} minutes and can only be used once.</p>
            <p><a href="{url}">Reset your password</a></p>
            <p>If you didn't request this, you can ignore this email; your password won't change.</p>
            """;

        var text = $"""
            Hello {(string.IsNullOrWhiteSpace(toName) ? toAddress : toName)},

            We received a request to reset your ISOFlow password. Open the link below to choose a new one.
            The link expires in {minutes} minutes and can only be used once.

            {resetUrl}

            If you didn't request this, you can ignore this email; your password won't change.
            """;

        return new EmailMessage(toAddress, toName, "Reset your ISOFlow password", html, text);
    }
}
