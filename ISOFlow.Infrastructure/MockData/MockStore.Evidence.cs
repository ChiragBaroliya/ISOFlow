using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Infrastructure.MockData;

public static partial class MockStore
{
    public static List<Evidence> Evidences { get; } = new()
    {
        new Evidence
        {
            Id = "EVI-2026-001", Code = "EVI-2026-001",
            Name = "Access_Review_Report_Q3_2026.pdf",
            Type = EvidenceType.Report,
            ControlId = "CTRL-001", RequirementId = "REQ-A5-18",
            TaskId = "TASK-2026-003", AuditId = "AUD-2026-001",
            UploadedBy = "David Chen",
            UploadDate = new DateTime(2026, 8, 25),
            ExpiryDate = new DateTime(2027, 8, 25),
            Status = "Verified",
            FileUrl = "/evidence/Access_Review_Report_Q3_2026.pdf"
        },
        new Evidence
        {
            Id = "EVI-2026-002", Code = "EVI-2026-002",
            Name = "Manager_Approval_Log_Aug2026.pdf",
            Type = EvidenceType.ApprovalRecord,
            ControlId = "CTRL-001", RequirementId = "REQ-A5-18",
            TaskId = "TASK-2026-003", AuditId = "AUD-2026-001",
            UploadedBy = "David Chen",
            UploadDate = new DateTime(2026, 8, 24),
            ExpiryDate = new DateTime(2027, 8, 24),
            Status = "Verified",
            FileUrl = "/evidence/Manager_Approval_Log_Aug2026.pdf"
        }
    };
}
