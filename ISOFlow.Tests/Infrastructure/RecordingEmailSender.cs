using ISOFlow.Application.Interfaces;

namespace ISOFlow.Tests.Infrastructure;

/// <summary>Test double for <see cref="IEmailSender"/> that records messages instead of delivering them.</summary>
public sealed class RecordingEmailSender : IEmailSender
{
    public List<EmailMessage> Sent { get; } = new();

    /// <summary>When set, every send throws this exception (simulates an SMTP outage).</summary>
    public Exception? FailWith { get; set; }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (FailWith != null) throw FailWith;
        Sent.Add(message);
        return Task.CompletedTask;
    }
}
