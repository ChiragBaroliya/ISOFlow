using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Entities;

public class TaskItem
{
    public string Id { get; set; } = string.Empty; // TASK-2026-003
    public int OrganizationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ControlId { get; set; } = string.Empty;
    public string RiskId { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public DateTime DueDate { get; set; }
    public ComplianceTaskStatus Status { get; set; }
    public string EvidenceId { get; set; } = string.Empty;
    public DateTime? CompletedDate { get; set; }
    public string Comments { get; set; } = string.Empty;
}
