using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Infrastructure.MockData;

public static partial class MockStore
{
    public static List<TaskTemplate> TaskTemplates { get; } = new()
    {
        new TaskTemplate { Id = "TPL-001", Code = "TPL-001", Title = "Quarterly User Access Review",  Frequency = "Quarterly", DefaultOwner = "David Chen",  RelatedControlId = "CTRL-001" },
        new TaskTemplate { Id = "TPL-002", Code = "TPL-002", Title = "Monthly Backup Recovery Test",  Frequency = "Monthly",   DefaultOwner = "David Chen",  RelatedControlId = "CTRL-006" },
        new TaskTemplate { Id = "TPL-003", Code = "TPL-003", Title = "Annual Risk Register Review",   Frequency = "Annual",    DefaultOwner = "Alex Morgan", RelatedControlId = "CTRL-001" }
    };

    public static List<TaskItem> Tasks { get; } = new()
    {
        new TaskItem
        {
            Id = "TASK-2026-003", Code = "TASK-2026-003",
            Title = "Q3 User Access Review",
            ControlId = "CTRL-001", RiskId = "RISK-001",
            Owner = "David Chen (IT Manager)",
            Priority = TaskPriority.High,
            DueDate = new DateTime(2026, 8, 30),
            Status = ComplianceTaskStatus.Completed,
            EvidenceId = "EVI-2026-001",
            CompletedDate = new DateTime(2026, 8, 25),
            Comments = "Completed Q3 user access audit across Okta, GitHub, and AWS."
        },
        new TaskItem
        {
            Id = "TASK-2026-004", Code = "TASK-2026-004",
            Title = "Q4 User Access Review",
            ControlId = "CTRL-001", RiskId = "RISK-001",
            Owner = "David Chen (IT Manager)",
            Priority = TaskPriority.High,
            DueDate = new DateTime(2026, 11, 30),
            Status = ComplianceTaskStatus.InProgress,
            Comments = "Preparing review lists for HR verification."
        },
        new TaskItem
        {
            Id = "TASK-2026-005", Code = "TASK-2026-005",
            Title = "Automate Access Lifecycle Integration",
            ControlId = "CTRL-001", RiskId = "RISK-001",
            Owner = "David Chen (IT Manager)",
            Priority = TaskPriority.Urgent,
            DueDate = new DateTime(2026, 10, 15),
            Status = ComplianceTaskStatus.InProgress,
            Comments = "Configuring Okta-Workday HR webhook automation."
        }
    };
}
