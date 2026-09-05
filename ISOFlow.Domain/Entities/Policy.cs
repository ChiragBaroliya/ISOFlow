namespace ISOFlow.Domain.Entities;

public class Policy
{
    public string Id { get; set; } = string.Empty; // POL-001
    public int OrganizationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public string Owner { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime NextReviewDate { get; set; }
    public string Status { get; set; } = "Active";
    public string FilePath { get; set; } = string.Empty;
    public List<string> LinkedControlIds { get; set; } = new();
    public List<string> LinkedProcessIds { get; set; } = new();
}
