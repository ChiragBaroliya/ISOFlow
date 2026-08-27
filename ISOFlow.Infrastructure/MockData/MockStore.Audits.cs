using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Infrastructure.MockData;

public static partial class MockStore
{
    public static List<AuditProgram> AuditPrograms { get; } = new()
    {
        new AuditProgram
        {
            Id = "PRG-2026", Title = "Annual ISO Compliance Audit Program 2026",
            Year = 2026, Scope = "All operational sites (Ahmedabad, Bengaluru, Amsterdam)",
            Status = "Active"
        }
    };

    public static List<Audit> Audits { get; } = new()
    {
        new Audit
        {
            Id = "AUD-2026-001", Code = "AUD-2026-001",
            Title = "ISO 27001 Internal Audit 2026",
            StandardId = "ISO-27001-2022",
            LeadAuditor = "Sarah Wilson",
            StartDate = new DateTime(2026, 8, 20),
            EndDate   = new DateTime(2026, 8, 25),
            Status = AuditStatus.InProgress,
            CompletionPercentage = 68,
            Scope = "Annex A Organizational Controls, Access Rights (A.5.18), Backup (A.8.13), and Incident Management.",
            CheckListControlIds = new() { "CTRL-001", "CTRL-002", "CTRL-003", "CTRL-004", "CTRL-005", "CTRL-006" },
            FindingIds = new() { "FIND-001" }
        }
    };

    public static List<Finding> Findings { get; } = new()
    {
        new Finding
        {
            Id = "FIND-001", Code = "FIND-001",
            Title = "Access not removed promptly for terminated employees",
            AuditId = "AUD-2026-001", RequirementId = "REQ-A5-18", ControlId = "CTRL-001",
            Severity = FindingSeverity.MajorNonConformity,
            Status = FindingStatus.CapaAssigned,
            Description = "During internal audit sample testing, 2 offboarded contractors retained GitHub repository access for 48 hours after contract termination date.",
            RootCause = "Manual communication breakdown between HR department and IT helpdesk via email.",
            IdentifiedDate = new DateTime(2026, 8, 22),
            Auditor = "Sarah Wilson",
            CapaId = "CAPA-001"
        }
    };

    public static List<CAPA> Capas { get; } = new()
    {
        new CAPA
        {
            Id = "CAPA-001", Code = "CAPA-001",
            FindingId = "FIND-001",
            Title = "Automate Employee Access Lifecycle Management",
            RootCause = "Manual email communication between HR and IT causing offboarding delays.",
            CorrectiveAction = "Implement automated identity lifecycle sync between Workday HR software and Okta IdP to trigger automatic account de-provisioning upon HR status change.",
            Owner = "David Chen (IT Manager)",
            DueDate = new DateTime(2026, 10, 15),
            Status = CapaStatus.InProgress,
            EffectivenessVerification = "Verification scheduled post-implementation via automated audit log check.",
            ActionItems = new()
            {
                new CapaActionItem { Id = "ACT-001", CapaId = "CAPA-001", Title = "Configure HR Integration API webhook",         AssignedTo = "David Chen",     DueDate = new DateTime(2026, 9, 15),  IsCompleted = true },
                new CapaActionItem { Id = "ACT-002", CapaId = "CAPA-001", Title = "Implement account disable automation workflow", AssignedTo = "David Chen",     DueDate = new DateTime(2026, 9, 30),  IsCompleted = true },
                new CapaActionItem { Id = "ACT-003", CapaId = "CAPA-001", Title = "Test automated offboarding scenario",          AssignedTo = "Sarah Wilson",   DueDate = new DateTime(2026, 10, 5),  IsCompleted = false },
                new CapaActionItem { Id = "ACT-004", CapaId = "CAPA-001", Title = "Update JML process documentation",             AssignedTo = "Chirag Baroliya", DueDate = new DateTime(2026, 10, 10), IsCompleted = false }
            }
        }
    };
}
