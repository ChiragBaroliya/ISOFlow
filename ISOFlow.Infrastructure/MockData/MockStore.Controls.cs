using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Infrastructure.MockData;

public static partial class MockStore
{
    public static List<Control> Controls { get; } = new()
    {
        new Control
        {
            Id = "CTRL-001", Code = "CTRL-001",
            Title = "User Access Management",
            RequirementId = "REQ-A5-18", StandardId = "ISO-27001-2022",
            Category = "Organizational Controls",
            Description = "Enforce strict Joiner-Mover-Leaver access lifecycle, role-based access control, and quarterly access reviews.",
            Status = ControlStatus.Implemented,
            Owner = "David Chen (IT Manager)", CompliancePercentage = 82.0,
            IsApplicable = true,
            Justification = "Critical control for protecting cloud and internal infrastructure access.",
            RelatedRequirementIds  = new() { "REQ-A5-18", "REQ-A5-15" },
            RelatedRiskIds         = new() { "RISK-001", "RISK-002" },
            RelatedTreatmentIds    = new() { "TRT-001" },
            RelatedPolicyIds       = new() { "POL-001" },
            RelatedProcessIds      = new() { "PROC-001" },
            RelatedTaskIds         = new() { "TASK-2026-001", "TASK-2026-002", "TASK-2026-003", "TASK-2026-004" },
            RelatedEvidenceIds     = new() { "EVI-2026-001", "EVI-2026-002" },
            RelatedAuditIds        = new() { "AUD-2026-001" },
            RelatedFindingIds      = new() { "FIND-001" },
            RelatedCapaIds         = new() { "CAPA-001" },
            RelatedManagementReviewIds = new() { "REV-2026-Q4" },
            RelatedImprovementIds  = new() { "IMP-001" }
        },
        new Control
        {
            Id = "CTRL-002", Code = "CTRL-002",
            Title = "Password Management",
            RequirementId = "REQ-A5-15", StandardId = "ISO-27001-2022",
            Category = "Organizational Controls",
            Description = "Enforce strong authentication, MFA, and password complexity policies.",
            Status = ControlStatus.Implemented,
            Owner = "David Chen (IT Manager)", CompliancePercentage = 95.0,
            IsApplicable = true, Justification = "Standard security posture requirement.",
            RelatedRequirementIds = new() { "REQ-A5-15" },
            RelatedRiskIds        = new() { "RISK-001" },
            RelatedTreatmentIds   = new() { "TRT-001" },
            RelatedPolicyIds      = new() { "POL-002" },
            RelatedProcessIds     = new() { "PROC-001" },
            RelatedTaskIds        = new() { "TASK-2026-005" },
            RelatedEvidenceIds    = new() { "EVI-2026-003" },
            RelatedAuditIds       = new() { "AUD-2026-001" }
        },
        new Control
        {
            Id = "CTRL-003", Code = "CTRL-003",
            Title = "Asset Management",
            RequirementId = "REQ-A8-1", StandardId = "ISO-27001-2022",
            Category = "Technological Controls",
            Description = "Maintain an up-to-date asset register for all laptops, servers, and software licenses.",
            Status = ControlStatus.Implemented,
            Owner = "David Chen (IT Manager)", CompliancePercentage = 90.0,
            IsApplicable = true, Justification = "Asset inventory tracking.",
            RelatedRequirementIds = new() { "REQ-A8-1" },
            RelatedRiskIds        = new() { "RISK-003" }
        },
        new Control
        {
            Id = "CTRL-004", Code = "CTRL-004",
            Title = "Supplier Security Assessment",
            RequirementId = "REQ-A5-19", StandardId = "ISO-27001-2022",
            Category = "Organizational Controls",
            Description = "Conduct annual security assessments of third-party vendors and SaaS providers.",
            Status = ControlStatus.InDevelopment,
            Owner = "Alex Morgan (Risk Manager)", CompliancePercentage = 75.0,
            IsApplicable = true, Justification = "Vendor risk management.",
            RelatedRequirementIds = new() { "REQ-A5-19" },
            RelatedRiskIds        = new() { "RISK-004" }
        },
        new Control
        {
            Id = "CTRL-005", Code = "CTRL-005",
            Title = "Incident Management",
            RequirementId = "REQ-A5-24", StandardId = "ISO-27001-2022",
            Category = "Organizational Controls",
            Description = "Detect, log, classify, and resolve security incidents within defined SLA.",
            Status = ControlStatus.Implemented,
            Owner = "Chirag Baroliya (Compliance Manager)", CompliancePercentage = 88.0,
            IsApplicable = true, Justification = "Incident response capability.",
            RelatedRequirementIds = new() { "REQ-A5-24" },
            RelatedRiskIds        = new() { "RISK-005" }
        },
        new Control
        {
            Id = "CTRL-006", Code = "CTRL-006",
            Title = "Backup & Recovery Management",
            RequirementId = "REQ-A8-13", StandardId = "ISO-27001-2022",
            Category = "Technological Controls",
            Description = "Daily automated encrypted backups with monthly restore testing drills.",
            Status = ControlStatus.Implemented,
            Owner = "David Chen (IT Manager)", CompliancePercentage = 95.0,
            IsApplicable = true, Justification = "Business continuity and disaster recovery.",
            RelatedRequirementIds = new() { "REQ-A8-13" },
            RelatedRiskIds        = new() { "RISK-006" }
        }
    };
}
