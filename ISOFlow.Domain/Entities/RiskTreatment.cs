using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Entities;

public class RiskTreatment
{
    public string Id { get; set; } = string.Empty; // TRT-001
    public string RiskId { get; set; } = string.Empty;
    public string Option { get; set; } = "Mitigate"; // Mitigate, Avoid, Transfer, Accept
    public string TreatmentPlan { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public RiskLikelihood ResidualLikelihood { get; set; }
    public RiskImpact ResidualImpact { get; set; }
    public int ResidualScore => (int)ResidualLikelihood * (int)ResidualImpact;
    public RiskLevel ResidualLevel => ResidualScore switch
    {
        >= 15 => RiskLevel.Critical,
        >= 10 => RiskLevel.High,
        >= 5 => RiskLevel.Medium,
        _ => RiskLevel.Low
    };
    public string Status { get; set; } = "In Progress";
}
