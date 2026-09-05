using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Entities;

public class Risk
{
    public string Id { get; set; } = string.Empty; // RISK-001
    public int OrganizationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Asset { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public RiskLikelihood Likelihood { get; set; }
    public RiskImpact Impact { get; set; }
    public int Score => (int)Likelihood * (int)Impact;
    public RiskLevel Level => Score switch
    {
        >= 15 => RiskLevel.Critical,
        >= 10 => RiskLevel.High,
        >= 5 => RiskLevel.Medium,
        _ => RiskLevel.Low
    };
    public string TreatmentId { get; set; } = string.Empty;
    public string ControlId { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
}
