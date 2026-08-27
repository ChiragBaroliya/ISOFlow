using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Infrastructure.MockData;

public static partial class MockStore
{
    public static List<Risk> Risks { get; } = new()
    {
        new Risk
        {
            Id = "RISK-001", Code = "RISK-001",
            Title = "Unauthorized System & Data Access",
            Description = "Former employees or un-offboarded staff retaining access to corporate SaaS applications.",
            Asset = "Customer Data & Cloud Infrastructure",
            Department = "IT & Engineering",
            Owner = "Alex Morgan (Risk Manager)",
            Likelihood = RiskLikelihood.Likely,   // 4
            Impact = RiskImpact.Major,             // 4 → Score 16 (Critical)
            TreatmentId = "TRT-001", ControlId = "CTRL-001", Status = "Mitigated"
        },
        new Risk
        {
            Id = "RISK-002", Code = "RISK-002",
            Title = "Excessive Admin Privileges",
            Description = "Developers having permanent production admin privileges.",
            Asset = "AWS Production Account",
            Department = "Engineering",
            Owner = "Alex Morgan (Risk Manager)",
            Likelihood = RiskLikelihood.Possible, // 3
            Impact = RiskImpact.Major,             // 4 → Score 12 (High)
            TreatmentId = "TRT-001", ControlId = "CTRL-001", Status = "Open"
        },
        new Risk
        {
            Id = "RISK-003", Code = "RISK-003",
            Title = "Laptop Theft & Unencrypted Drive",
            Description = "Physical theft of employee laptops containing sensitive source code.",
            Asset = "Employee Laptops",
            Department = "All Departments",
            Owner = "David Chen (IT Manager)",
            Likelihood = RiskLikelihood.Unlikely, // 2
            Impact = RiskImpact.Moderate,          // 3 → Score 6 (Medium)
            TreatmentId = "", ControlId = "CTRL-003", Status = "Closed"
        },
        new Risk
        {
            Id = "RISK-004", Code = "RISK-004",
            Title = "Third-Party SaaS Breach",
            Description = "Security breach at key SaaS supplier exposing shared customer metadata.",
            Asset = "Vendor Services",
            Department = "Procurement",
            Owner = "Alex Morgan (Risk Manager)",
            Likelihood = RiskLikelihood.Possible, // 3
            Impact = RiskImpact.Major,             // 4 → Score 12 (High)
            TreatmentId = "", ControlId = "CTRL-004", Status = "Open"
        },
        new Risk
        {
            Id = "RISK-005", Code = "RISK-005",
            Title = "Delayed Incident Escalation",
            Description = "Failure to report security anomalies within 24 hours.",
            Asset = "Security Operations",
            Department = "InfoSec",
            Owner = "Chirag Baroliya (Compliance Manager)",
            Likelihood = RiskLikelihood.Unlikely, // 2
            Impact = RiskImpact.Minor,             // 2 → Score 4 (Low)
            TreatmentId = "", ControlId = "CTRL-005", Status = "Mitigated"
        }
    };

    public static List<RiskTreatment> RiskTreatments { get; } = new()
    {
        new RiskTreatment
        {
            Id = "TRT-001", RiskId = "RISK-001",
            Option = "Mitigate",
            TreatmentPlan = "Implement automated Joiner-Mover-Leaver workflow integrated with HR system (Workday/Okta).",
            Owner = "David Chen (IT Manager)",
            TargetDate = new DateTime(2026, 10, 15),
            ResidualLikelihood = RiskLikelihood.Rare,  // 1
            ResidualImpact = RiskImpact.Minor,          // 2 → Score 2 (Low)
            Status = "In Progress"
        }
    };
}
