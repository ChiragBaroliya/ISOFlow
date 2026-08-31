namespace ISOFlow.Domain.Entities;

public class Process
{
    public string Id { get; set; } = string.Empty; // PROC-001
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public string Status { get; set; } = "Active";
    public List<string> Steps { get; set; } = new();
    public string PolicyId { get; set; } = string.Empty;
    public List<string> ControlIds { get; set; } = new();
}
