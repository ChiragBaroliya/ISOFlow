using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Infrastructure.MockData;

public static partial class MockStore
{
    /// <summary>All platform users. SuperAdmin has empty OrganizationId (global scope).</summary>
    public static List<User> Users { get; } = new()
    {
        // ── Super Admin (Global — not scoped to any Organization) ────────────────
        new User
        {
            Id = "USR-SA-001", OrganizationId = "",
            Name = "Super Admin", Email = "superadmin@isoflow.io", Password = "Test@123",
            SystemRole = SystemRole.SuperAdmin, Role = "Platform Administrator",
            Department = "ISOFlow Platform", Location = "Global",
            Phone = "+1 (800) 123-4567", Bio = "Global platform administrator with full access to all organizations and system settings.",
            AvatarUrl = "https://ui-avatars.com/api/?name=Super+Admin&background=7c3aed&color=fff"
        },

        // ── ORG-001 : Acme Technologies ──────────────────────────────────────────
        new User
        {
            Id = "USR-001", OrganizationId = "ORG-001",
            Name = "Sarah Wilson", Email = "sarah.wilson@acme.com", Password = "Test@123",
            SystemRole = SystemRole.Admin, Role = "Compliance Manager",
            Department = "Compliance & Quality", Location = "Amsterdam",
            Phone = "+31 20 555 0101", Bio = "ISO 27001 Lead Auditor with 8 years of experience managing compliance programs across EMEA.",
            AvatarUrl = "https://ui-avatars.com/api/?name=Sarah+Wilson&background=4f46e5&color=fff"
        },
        new User
        {
            Id = "USR-002", OrganizationId = "ORG-001",
            Name = "Chirag Baroliya", Email = "chirag.dev@acme.com", Password = "Test@123",
            SystemRole = SystemRole.User, Role = "ISO Auditor",
            Department = "Information Security", Location = "Ahmedabad",
            Phone = "+91 79 555 0202", Bio = "Information security specialist focused on ISO 27001 controls implementation and evidence collection.",
            AvatarUrl = "https://ui-avatars.com/api/?name=Chirag+Baroliya&background=0d9488&color=fff"
        },
        new User
        {
            Id = "USR-003", OrganizationId = "ORG-001",
            Name = "Alex Morgan", Email = "alex.morgan@acme.com", Password = "Test@123",
            SystemRole = SystemRole.User, Role = "Risk Manager",
            Department = "Risk & Governance", Location = "Bengaluru",
            Phone = "+91 80 555 0303", Bio = "Enterprise risk management professional specializing in cybersecurity risk assessment and treatment planning.",
            AvatarUrl = "https://ui-avatars.com/api/?name=Alex+Morgan&background=d97706&color=fff"
        },
        new User
        {
            Id = "USR-004", OrganizationId = "ORG-001",
            Name = "David Chen", Email = "david.chen@acme.com", Password = "Test@123",
            SystemRole = SystemRole.User, Role = "IT Manager",
            Department = "IT Infrastructure", Location = "Ahmedabad",
            Phone = "+91 79 555 0404", Bio = "IT infrastructure manager responsible for access control systems, cloud security, and backup operations.",
            AvatarUrl = "https://ui-avatars.com/api/?name=David+Chen&background=2563eb&color=fff"
        },

        // ── ORG-002 : CyberShield Global Security ────────────────────────────────
        new User
        {
            Id = "USR-005", OrganizationId = "ORG-002",
            Name = "Marcus Vance", Email = "marcus.v@cybershield.com", Password = "Test@123",
            SystemRole = SystemRole.Admin, Role = "CISO",
            Department = "Cybersecurity", Location = "London",
            Phone = "+44 20 555 0505", Bio = "Chief Information Security Officer with 15 years in enterprise cybersecurity and ISO 27001 certification leadership.",
            AvatarUrl = "https://ui-avatars.com/api/?name=Marcus+Vance&background=dc2626&color=fff"
        },

        // ── ORG-003 : Nexus Health Systems ───────────────────────────────────────
        new User
        {
            Id = "USR-006", OrganizationId = "ORG-003",
            Name = "Dr. Julia Weber", Email = "j.weber@nexushealth.org", Password = "Test@123",
            SystemRole = SystemRole.Admin, Role = "Quality Lead",
            Department = "Medical Compliance", Location = "Zurich",
            Phone = "+41 44 555 0606", Bio = "Medical compliance specialist and ISO 9001 Quality Management System lead for healthcare regulatory adherence.",
            AvatarUrl = "https://ui-avatars.com/api/?name=Julia+Weber&background=059669&color=fff"
        }
    };
}
