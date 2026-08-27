// ─────────────────────────────────────────────────────────────────────────────
// MockStore — In-Memory Mock Data Store for ISOFlow
//
// ARCHITECTURE NOTE:
//   All platform mock data lives here, split into focused partial-class files:
//     MockStore.Organizations.cs  MockStore.Users.cs       MockStore.Standards.cs
//     MockStore.Controls.cs       MockStore.Risks.cs       MockStore.Documents.cs
//     MockStore.Tasks.cs          MockStore.Evidence.cs    MockStore.Audits.cs
//     MockStore.Reviews.cs        MockStore.System.cs
//
// FUTURE API / DATABASE MIGRATION:
//   When connecting to a real API or database, ONLY ISOFlow.Infrastructure changes:
//     1. Delete this MockData/ folder.
//     2. Rewrite each Repository in Repositories/ to use EF Core / HttpClient.
//     3. Update DI registrations in Program.cs.
//   → Domain, Application, and Web layers remain 100% untouched.
// ─────────────────────────────────────────────────────────────────────────────

using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Infrastructure.MockData;

/// <summary>
/// Central in-memory mock data store.
/// Each partial file owns one domain area.
/// All lists are mutable so repositories can simulate create/update/delete.
/// </summary>
public static partial class MockStore
{
    // Shared helper — used by repositories for ID generation
    public static string NextId(string prefix, int current) =>
        $"{prefix}-{(current + 1):D3}";
}
