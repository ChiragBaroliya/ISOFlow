using ISOFlow.Domain.Entities;

namespace ISOFlow.Infrastructure.MockData;

public static partial class MockStore
{
    public static List<Organization> Organizations { get; } = new()
    {
        new Organization
        {
            Id = "ORG-001",
            Code = "ACME",
            Name = "Acme Technologies Pvt. Ltd.",
            Industry = "Technology / SaaS",
            Employees = 250,
            Locations = new() { "Ahmedabad", "Bengaluru", "Amsterdam" },
            PrimaryStandard = "ISO 27001:2022",
            Status = "Active",
            CompliancePercentage = 85.5,
            ContactEmail = "chirag.dev@acme.com",
            CreatedAt = new DateTime(2025, 1, 10)
        },
        new Organization
        {
            Id = "ORG-002",
            Code = "CYBER",
            Name = "CyberShield Global Security",
            Industry = "Cybersecurity Services",
            Employees = 1200,
            Locations = new() { "London", "New York", "Singapore" },
            PrimaryStandard = "ISO 27001:2022",
            Status = "Active",
            CompliancePercentage = 92.0,
            ContactEmail = "info@cybershield.com",
            CreatedAt = new DateTime(2025, 3, 15)
        },
        new Organization
        {
            Id = "ORG-003",
            Code = "NEXUS",
            Name = "Nexus Health Systems",
            Industry = "Healthcare & Life Sciences",
            Employees = 450,
            Locations = new() { "Berlin", "Zurich", "Boston" },
            PrimaryStandard = "ISO 9001:2015",
            Status = "Active",
            CompliancePercentage = 89.5,
            ContactEmail = "compliance@nexushealth.org",
            CreatedAt = new DateTime(2025, 6, 20)
        }
    };
}
