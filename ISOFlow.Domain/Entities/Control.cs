using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Entities;

public class Control
{
    public string Id { get; set; } = string.Empty; // e.g., CTRL-001
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string RequirementId { get; set; } = string.Empty;
    public string StandardId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ControlStatus Status { get; set; }
    public string Owner { get; set; } = string.Empty;
    public double CompliancePercentage { get; set; }
    public bool IsApplicable { get; set; } = true;
    public string Justification { get; set; } = string.Empty;

    // Cross-module linking IDs
    public List<string> RelatedRequirementIds { get; set; } = new();
    public List<string> RelatedRiskIds { get; set; } = new();
    public List<string> RelatedTreatmentIds { get; set; } = new();
    public List<string> RelatedPolicyIds { get; set; } = new();
    public List<string> RelatedProcessIds { get; set; } = new();
    public List<string> RelatedTaskIds { get; set; } = new();
    public List<string> RelatedEvidenceIds { get; set; } = new();
    public List<string> RelatedAuditIds { get; set; } = new();
    public List<string> RelatedFindingIds { get; set; } = new();
    public List<string> RelatedCapaIds { get; set; } = new();
    public List<string> RelatedManagementReviewIds { get; set; } = new();
    public List<string> RelatedImprovementIds { get; set; } = new();
}
