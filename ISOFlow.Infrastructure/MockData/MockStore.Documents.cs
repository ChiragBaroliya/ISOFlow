using ISOFlow.Domain.Entities;

namespace ISOFlow.Infrastructure.MockData;

public static partial class MockStore
{
    public static List<Policy> Policies { get; } = new()
    {
        new Policy
        {
            Id = "POL-001", Code = "POL-001",
            Title = "Access Control Policy", Version = "2.1",
            Owner = "Chirag Baroliya (Compliance Manager)",
            EffectiveDate = new DateTime(2026, 1, 15),
            NextReviewDate = new DateTime(2027, 1, 15),
            Status = "Active",
            FilePath = "/docs/Access_Control_Policy_v2.1.pdf",
            LinkedControlIds = new() { "CTRL-001", "CTRL-002" },
            LinkedProcessIds = new() { "PROC-001" }
        },
        new Policy
        {
            Id = "POL-002", Code = "POL-002",
            Title = "Password & Credential Policy", Version = "1.4",
            Owner = "David Chen (IT Manager)",
            EffectiveDate = new DateTime(2025, 11, 1),
            NextReviewDate = new DateTime(2026, 11, 1),
            Status = "Active",
            FilePath = "/docs/Password_Policy_v1.4.pdf",
            LinkedControlIds = new() { "CTRL-002" }
        },
        new Policy
        {
            Id = "POL-003", Code = "POL-003",
            Title = "Information Security Policy", Version = "3.0",
            Owner = "Chirag Baroliya (Compliance Manager)",
            EffectiveDate = new DateTime(2026, 2, 1),
            NextReviewDate = new DateTime(2027, 2, 1),
            Status = "Active",
            FilePath = "/docs/Information_Security_Policy.pdf",
            LinkedControlIds = new() { "CTRL-001", "CTRL-003", "CTRL-005" }
        }
    };

    public static List<Process> Processes { get; } = new()
    {
        new Process
        {
            Id = "PROC-001", Code = "PROC-001",
            Title = "Joiner-Mover-Leaver Process",
            Category = "User Access Lifecycle",
            Owner = "David Chen (IT Manager)",
            Description = "Standard operating procedure for provisioning, modifying, and de-provisioning employee access across all systems.",
            Steps = new()
            {
                "1. Employee Joins (HR triggers onboarding ticket)",
                "2. Access Request submitted by manager",
                "3. Manager & InfoSec Approval",
                "4. IT Access Provisioning via SSO",
                "5. Periodic Quarterly Access Review",
                "6. Employee Leaves (Immediate revocation within 4 hours of termination)"
            },
            PolicyId = "POL-001",
            ControlIds = new() { "CTRL-001" }
        }
    };
}
