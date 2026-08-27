using ISOFlow.Domain.Enums;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Infrastructure.MockData;

public static partial class MockStore
{
    public static List<Standard> Standards { get; } = new()
    {
        new Standard
        {
            Id = "ISO-27001-2022", Code = "ISO 27001:2022",
            Name = "Information Security Management System (ISMS)",
            Revision = "2022",
            Description = "International standard for information security management.",
            RequirementCount = 93, CompliancePercentage = 82.5,
            IsPreseeded = true, Status = "Active"
        },
        new Standard
        {
            Id = "ISO-9001-2015", Code = "ISO 9001:2015",
            Name = "Quality Management System (QMS)",
            Revision = "2015",
            Description = "International standard for quality management systems.",
            RequirementCount = 45, CompliancePercentage = 91.0,
            IsPreseeded = true, Status = "Active"
        },
        new Standard
        {
            Id = "ISO-14001-2015", Code = "ISO 14001:2015",
            Name = "Environmental Management System (EMS)",
            Revision = "2015",
            Description = "International standard for environmental compliance.",
            RequirementCount = 38, CompliancePercentage = 88.0,
            IsPreseeded = true, Status = "Active"
        }
    };

    public static List<Requirement> Requirements { get; } = new()
    {
        new Requirement { Id = "REQ-A5-18", StandardId = "ISO-27001-2022", Clause = "A.5.18", Title = "Access Rights", Category = "Organizational Controls", Description = "Access rights to information and other associated assets shall be provisioned, reviewed, modified and removed in accordance with the organization's topic-specific policy on access control.", CompliancePercentage = 80.0, RelatedControlIds = new() { "CTRL-001" } },
        new Requirement { Id = "REQ-A5-15", StandardId = "ISO-27001-2022", Clause = "A.5.15", Title = "Access Control", Category = "Organizational Controls", Description = "Rules to control physical and logical access to information shall be established based on business requirements.", CompliancePercentage = 85.0, RelatedControlIds = new() { "CTRL-001", "CTRL-002" } },
        new Requirement { Id = "REQ-A8-1",  StandardId = "ISO-27001-2022", Clause = "A.8.1",  Title = "User Endpoint Devices", Category = "Technological Controls", Description = "Information stored on, processed by or accessible via user endpoint devices shall be protected.", CompliancePercentage = 90.0, RelatedControlIds = new() { "CTRL-003" } },
        new Requirement { Id = "REQ-A5-19", StandardId = "ISO-27001-2022", Clause = "A.5.19", Title = "Information Security in Supplier Relationships", Category = "Organizational Controls", Description = "Processes shall be defined to manage risks associated with supplier access to assets.", CompliancePercentage = 75.0, RelatedControlIds = new() { "CTRL-004" } },
        new Requirement { Id = "REQ-A5-24", StandardId = "ISO-27001-2022", Clause = "A.5.24", Title = "Incident Management Planning", Category = "Organizational Controls", Description = "Organization shall plan and prepare for managing security incidents.", CompliancePercentage = 88.0, RelatedControlIds = new() { "CTRL-005" } },
        new Requirement { Id = "REQ-A8-13", StandardId = "ISO-27001-2022", Clause = "A.8.13", Title = "Information Backup", Category = "Technological Controls", Description = "Backup copies of information, software and system images shall be taken and tested regularly.", CompliancePercentage = 95.0, RelatedControlIds = new() { "CTRL-006" } },
        new Requirement { Id = "REQ-CLAUSE-4",  StandardId = "ISO-27001-2022", Clause = "Clause 4",  Title = "Context of the Organization", Category = "Management Clauses", Description = "Understanding the organization and its context, needs and expectations of interested parties.", CompliancePercentage = 100.0, RelatedControlIds = new() { "CTRL-001" } },
        new Requirement { Id = "REQ-CLAUSE-5",  StandardId = "ISO-27001-2022", Clause = "Clause 5",  Title = "Leadership",           Category = "Management Clauses", Description = "Leadership commitment, policy establishment, roles and responsibilities.", CompliancePercentage = 95.0, RelatedControlIds = new() { "CTRL-001" } },
        new Requirement { Id = "REQ-CLAUSE-6",  StandardId = "ISO-27001-2022", Clause = "Clause 6",  Title = "Planning",             Category = "Management Clauses", Description = "Actions to address risks and opportunities, information security objectives.", CompliancePercentage = 82.0, RelatedControlIds = new() { "CTRL-001" } },
        new Requirement { Id = "REQ-CLAUSE-9",  StandardId = "ISO-27001-2022", Clause = "Clause 9",  Title = "Performance Evaluation", Category = "Management Clauses", Description = "Monitoring, measurement, analysis, evaluation, internal audit and management review.", CompliancePercentage = 78.0, RelatedControlIds = new() { "CTRL-001" } },
        new Requirement { Id = "REQ-CLAUSE-10", StandardId = "ISO-27001-2022", Clause = "Clause 10", Title = "Improvement",          Category = "Management Clauses", Description = "Continual improvement and nonconformity corrective actions.", CompliancePercentage = 85.0, RelatedControlIds = new() { "CTRL-001" } }
    };
}
