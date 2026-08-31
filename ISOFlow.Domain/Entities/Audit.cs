using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Entities;

public class Audit
{
    public string Id { get; set; } = string.Empty; // AUD-2026-001
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string StandardId { get; set; } = string.Empty;
    public string LeadAuditor { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AuditStatus Status { get; set; }
    public int CompletionPercentage { get; set; }
    public string Scope { get; set; } = string.Empty;
    public List<string> CheckListControlIds { get; set; } = new();
    public List<string> FindingIds { get; set; } = new();
}
