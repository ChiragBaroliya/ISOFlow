namespace ISOFlow.Domain.Entities;

public class Organization
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public int Employees { get; set; }
    public List<string> Locations { get; set; } = new();
    public string PrimaryStandard { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public double CompliancePercentage { get; set; } = 85.0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ContactEmail { get; set; } = string.Empty;
}
