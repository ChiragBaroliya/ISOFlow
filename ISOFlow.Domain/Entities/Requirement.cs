namespace ISOFlow.Domain.Entities;

public class Requirement
{
    public string Id { get; set; } = string.Empty;
    public string StandardId { get; set; } = string.Empty;
    public string Clause { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // e.g. Organizational Controls, Technological Controls
    public double CompliancePercentage { get; set; }
    public List<string> RelatedControlIds { get; set; } = new();
}
