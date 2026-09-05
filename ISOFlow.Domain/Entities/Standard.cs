namespace ISOFlow.Domain.Entities;

public class Standard
{
    public string Id { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequirementCount { get; set; }
    public double CompliancePercentage { get; set; }
    public bool IsPreseeded { get; set; } = false;
    public string Status { get; set; } = "Active";
}
