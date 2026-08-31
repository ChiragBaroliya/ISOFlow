namespace ISOFlow.Domain.Entities;

public class Notification
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // Warning, Task, Policy, Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
    public string LinkUrl { get; set; } = string.Empty;
}
