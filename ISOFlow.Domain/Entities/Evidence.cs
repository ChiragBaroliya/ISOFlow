using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Entities;

public class Evidence
{
    public string Id { get; set; } = string.Empty; // EVI-2026-001
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public EvidenceType Type { get; set; }
    public string ControlId { get; set; } = string.Empty;
    public string RequirementId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string AuditId { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = "Verified";
    public string FileUrl { get; set; } = string.Empty;
}
