using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Entities;

public class Finding
{
    public string Id { get; set; } = string.Empty; // FIND-001
    public int OrganizationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string AuditId { get; set; } = string.Empty;
    public string RequirementId { get; set; } = string.Empty;
    public string ControlId { get; set; } = string.Empty;
    public FindingSeverity Severity { get; set; }
    public FindingStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public DateTime IdentifiedDate { get; set; }
    public string Auditor { get; set; } = string.Empty;
    public string CapaId { get; set; } = string.Empty;
}
