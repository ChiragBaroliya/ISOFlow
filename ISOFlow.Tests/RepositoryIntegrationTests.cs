using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Helpers;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;
using ISOFlow.Infrastructure.Repositories;
using ISOFlow.Tests.Infrastructure;

namespace ISOFlow.Tests;

/// <summary>
/// PostgreSQL integration tests for the repositories not covered by <see cref="DatabaseCrudTests"/>.
/// Run against the seeded test database (see <see cref="TestDatabase"/>); every test creates its own
/// uniquely-coded rows so tests stay independent of each other.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class RepositoryIntegrationTests
{
    private const int Org = 1;
    private const int OtherOrg = 2;

    // Resolved lazily so constructing the class never needs a database (tests are skipped without one).
    private static IDbConnectionFactory Db => TestDatabase.CreateFactory();

    private static string Unique(string prefix) => $"{prefix}-T{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

    // ── Users: hashed storage, login, password change and reset-token lifecycle ─────────────────

    [DatabaseFact]
    public async Task UserRepository_StoresOnlyHashes_AndValidatesLogin()
    {
        var repo = new UserRepository(Db);
        var email = $"{Unique("user").ToLowerInvariant()}@test.local";

        var created = await repo.CreateUserAsync(new User
        {
            OrganizationId = Org, Name = "Integration User", Email = email,
            PasswordHash = PasswordSecurityHelper.HashPassword("Initial#Pass1"),
            SystemRole = SystemRole.User, Role = "Analyst", Department = "QA", Location = "Remote"
        });

        var fetched = await repo.GetUserByEmailAsync(email);
        Assert.NotNull(fetched);
        Assert.True(PasswordSecurityHelper.IsPasswordHash(fetched.PasswordHash));
        Assert.NotNull(await repo.ValidateLoginAsync(email, "Initial#Pass1"));
        Assert.Null(await repo.ValidateLoginAsync(email, "wrong-password"));

        Assert.False(await repo.ChangePasswordAsync(created.Id, "wrong-current", "Next#Pass2"));
        Assert.True(await repo.ChangePasswordAsync(created.Id, "Initial#Pass1", "Next#Pass2"));
        Assert.NotNull(await repo.ValidateLoginAsync(email, "Next#Pass2"));
        Assert.Null(await repo.ValidateLoginAsync(email, "Initial#Pass1"));

        Assert.True(await repo.DeleteUserAsync(created.Id));
        Assert.Null(await repo.GetUserByIdAsync(created.Id));
    }

    [DatabaseFact]
    public async Task UserRepository_PlainTextPasswordHash_IsRejectedOnWrite()
    {
        var repo = new UserRepository(Db);

        await Assert.ThrowsAsync<ArgumentException>(() => repo.CreateUserAsync(new User
        {
            OrganizationId = Org, Name = "Plain", Email = $"{Unique("plain").ToLowerInvariant()}@test.local",
            PasswordHash = "Test@123", Role = "Analyst", Department = "QA", Location = "Remote"
        }));
    }

    [DatabaseFact]
    public async Task UserRepository_ResetToken_IsHashedSingleUseAndExpires()
    {
        var repo = new UserRepository(Db);
        var email = $"{Unique("reset").ToLowerInvariant()}@test.local";
        var user = await repo.CreateUserAsync(new User
        {
            OrganizationId = Org, Name = "Reset User", Email = email,
            PasswordHash = PasswordSecurityHelper.HashPassword("Before#Reset1"),
            Role = "Analyst", Department = "QA", Location = "Remote"
        });

        Assert.Null(await repo.GenerateResetTokenAsync("nobody@test.local"));

        // Only the SHA-256 digest is stored, with a future expiry.
        var token = await repo.GenerateResetTokenAsync(email);
        Assert.NotNull(token);
        var stored = await repo.GetUserByIdAsync(user.Id);
        Assert.Equal(PasswordSecurityHelper.HashResetToken(token), stored!.ResetTokenHash);
        Assert.NotEqual(token, stored.ResetTokenHash);
        Assert.True(stored.ResetTokenExpiresAt > DateTime.UtcNow);

        // Single use.
        Assert.True(await repo.ResetPasswordAsync(token, "After#Reset2"));
        Assert.False(await repo.ResetPasswordAsync(token, "Again#Reset3"));
        Assert.NotNull(await repo.ValidateLoginAsync(email, "After#Reset2"));

        // Expired tokens are rejected.
        var expired = await repo.GenerateResetTokenAsync(email);
        using (var conn = await Db.CreateOpenConnectionAsync())
        {
            await conn.ExecuteAsync("UPDATE users SET reset_token_expires_at = @past WHERE id = @id",
                new { past = DateTime.UtcNow.AddMinutes(-1), id = int.Parse(user.Id) });
        }
        Assert.False(await repo.ResetPasswordAsync(expired!, "Expired#Reset4"));

        await repo.DeleteUserAsync(user.Id);
    }

    // ── Refresh tokens ──────────────────────────────────────────────────────────────────────────

    [DatabaseFact]
    public async Task RefreshTokenRepository_CreateReadRevoke()
    {
        var repo = new RefreshTokenRepository(Db);
        var hash = Unique("rt-hash");

        await repo.CreateAsync(new RefreshToken
        {
            UserId = "2", TokenHash = hash, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        var fetched = await repo.GetByTokenHashAsync(hash);
        Assert.NotNull(fetched);
        Assert.Null(fetched.RevokedAt);

        Assert.True(await repo.RevokeByTokenHashAsync(hash, "replacement-hash"));
        Assert.False(await repo.RevokeByTokenHashAsync(hash)); // already revoked
        var revoked = await repo.GetByTokenHashAsync(hash);
        Assert.NotNull(revoked!.RevokedAt);
        Assert.Equal("replacement-hash", revoked.ReplacedByTokenHash);

        Assert.Null(await repo.GetByTokenHashAsync("does-not-exist"));
    }

    [DatabaseFact]
    public async Task RefreshTokenRepository_InvalidUser_FailsInsteadOfPretendingSuccess()
    {
        var repo = new RefreshTokenRepository(Db);

        // user 999999 does not exist: the FK violation must surface, not be swallowed.
        await Assert.ThrowsAnyAsync<Exception>(() => repo.CreateAsync(new RefreshToken
        {
            UserId = "999999", TokenHash = Unique("rt-orphan"), CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(1)
        }));
    }

    // ── Organizations ───────────────────────────────────────────────────────────────────────────

    [DatabaseFact]
    public async Task OrganizationRepository_Crud()
    {
        var repo = new OrganizationRepository(Db);
        var code = Unique("ORG");

        var created = await repo.CreateOrganizationAsync(new Organization
        {
            Code = code, Name = "Integration Test Org", Industry = "Testing", Employees = 10,
            Locations = new() { "Remote" }, PrimaryStandard = "ISO/IEC 27001:2022", ContactEmail = "org@test.local"
        });
        Assert.True(created.Id > 0);

        var fetched = await repo.GetOrganizationByIdAsync(created.Id.ToString());
        Assert.Equal(code, fetched!.Code);
        Assert.Equal(new[] { "Remote" }, fetched.Locations);

        fetched.Name = "Renamed Test Org";
        fetched.Locations = new() { "Remote", "Pune" };
        Assert.Equal("Renamed Test Org", (await repo.UpdateOrganizationAsync(fetched))!.Name);
        Assert.Equal(new[] { "Remote", "Pune" }, (await repo.GetOrganizationByIdAsync(created.Id.ToString()))!.Locations);

        Assert.True(await repo.DeleteOrganizationAsync(created.Id.ToString()));
        Assert.Null(await repo.GetOrganizationByIdAsync(created.Id.ToString()));
    }

    // ── Audits & findings, including tenant isolation ───────────────────────────────────────────

    [DatabaseFact]
    public async Task AuditRepository_AuditAndFindingCrud_AreTenantScoped()
    {
        var repo = new AuditRepository(Db);

        var audit = await repo.CreateAuditAsync(new Audit
        {
            OrganizationId = Org, Code = Unique("AUD"), Title = "Integration Audit", StandardId = "1",
            LeadAuditor = "Tester", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5),
            Status = AuditStatus.Planned, Scope = "Test scope", CheckListControlIds = new() { "CTRL-001" }
        });
        var fetchedAudit = await repo.GetAuditByIdAsync(audit.Id, Org);
        Assert.Equal(new[] { "CTRL-001" }, fetchedAudit!.CheckListControlIds);
        Assert.Contains((await repo.GetAllAuditsAsync(Org)).Single(a => a.Id == audit.Id).CheckListControlIds, c => c == "CTRL-001");
        Assert.Null(await repo.GetAuditByIdAsync(audit.Id, OtherOrg));

        var finding = await repo.CreateFindingAsync(new Finding
        {
            OrganizationId = Org, Code = Unique("FIND"), Title = "Integration Finding", AuditId = audit.Id,
            ControlId = "1", Severity = FindingSeverity.Observation, Status = FindingStatus.Open,
            Description = "desc", IdentifiedDate = DateTime.Today, Auditor = "Tester"
        });
        var fetchedFinding = await repo.GetFindingByIdAsync(finding.Id, Org);
        Assert.Equal(audit.Id, fetchedFinding!.AuditId);

        // Another tenant can neither change nor delete these rows.
        audit.Title = "Hijacked";
        Assert.Null(await repo.UpdateAuditAsync(audit, OtherOrg));
        Assert.False(await repo.DeleteFindingAsync(finding.Id, OtherOrg));
        Assert.False(await repo.DeleteAuditAsync(audit.Id, OtherOrg));

        audit.Title = "Updated Integration Audit";
        Assert.Equal("Updated Integration Audit", (await repo.UpdateAuditAsync(audit, Org))!.Title);

        Assert.True(await repo.DeleteFindingAsync(finding.Id, Org));
        Assert.True(await repo.DeleteAuditAsync(audit.Id, Org));
        Assert.Null(await repo.GetAuditByIdAsync(audit.Id, Org));
    }

    // ── JSON link lists on requirements and controls (seeded + round-trip) ──────────────────────

    [DatabaseFact]
    public async Task StandardAndControlRepositories_PersistAndLoadLinkLists()
    {
        var standards = new StandardRepository(Db);
        var controls = new ControlRepository(Db);

        // Seed data: requirement A.5.18 links CTRL-001.
        var seededRequirement = (await standards.GetRequirementsByStandardIdAsync("1", Org)).Single(r => r.Clause == "A.5.18");
        Assert.Contains("CTRL-001", seededRequirement.RelatedControlIds);

        var requirement = await standards.CreateRequirementAsync(new Requirement
        {
            OrganizationId = Org, StandardId = "1", Clause = Unique("T"), Title = "Integration Requirement",
            Description = "desc", Category = "Testing", RelatedControlIds = new() { "CTRL-001", "CTRL-002" }
        });
        Assert.Equal(new[] { "CTRL-001", "CTRL-002" }, (await standards.GetRequirementByIdAsync(requirement.Id, Org))!.RelatedControlIds);
        Assert.True(await standards.DeleteRequirementAsync(requirement.Id, Org));

        var control = await controls.CreateControlAsync(new Control
        {
            OrganizationId = Org, Code = Unique("CTRL"), Title = "Integration Control", StandardId = "1", RequirementId = "1",
            Category = "Testing", Description = "desc", Owner = "Tester",
            RelatedRiskIds = new() { "RISK-001" }, RelatedPolicyIds = new() { "POL-001" }
        });
        var fetched = await controls.GetControlByIdAsync(control.Id, Org);
        Assert.Equal(new[] { "RISK-001" }, fetched!.RelatedRiskIds);
        Assert.Equal(new[] { "POL-001" }, fetched.RelatedPolicyIds);

        fetched.RelatedRiskIds = new() { "RISK-001", "RISK-002" };
        await controls.UpdateControlAsync(fetched, Org);
        Assert.Equal(2, (await controls.GetControlByIdAsync(control.Id, Org))!.RelatedRiskIds.Count);
        Assert.True(await controls.DeleteControlAsync(control.Id, Org));
    }

    // ── CAPA with action items ──────────────────────────────────────────────────────────────────

    [DatabaseFact]
    public async Task CapaRepository_CrudAndActionItems()
    {
        var repo = new CapaRepository(Db);

        var capa = await repo.CreateCapaAsync(new CAPA
        {
            OrganizationId = Org, Code = Unique("CAPA"), FindingId = "1", Title = "Integration CAPA",
            RootCause = "rc", CorrectiveAction = "ca", Owner = "Tester", DueDate = DateTime.Today.AddDays(30),
            Status = CapaStatus.Draft
        });

        Assert.True(await repo.AddActionItemAsync(capa.Id, new CapaActionItem
        {
            Title = "Do the thing", AssignedTo = "Tester", DueDate = DateTime.Today.AddDays(7)
        }, Org));

        var withItem = await repo.GetCapaByIdAsync(capa.Id, Org);
        var item = Assert.Single(withItem!.ActionItems);
        Assert.False(item.IsCompleted);

        Assert.True(await repo.ToggleActionItemAsync(capa.Id, item.Id, Org));
        Assert.True(Assert.Single((await repo.GetCapaByIdAsync(capa.Id, Org))!.ActionItems).IsCompleted);

        Assert.Null(await repo.GetCapaByIdAsync(capa.Id, OtherOrg));
        Assert.True(await repo.DeleteCapaAsync(capa.Id, Org));
        Assert.Null(await repo.GetCapaByIdAsync(capa.Id, Org));
    }

    // ── Evidence ────────────────────────────────────────────────────────────────────────────────

    [DatabaseFact]
    public async Task EvidenceRepository_Crud()
    {
        var repo = new EvidenceRepository(Db);

        var evidence = await repo.CreateEvidenceAsync(new Evidence
        {
            OrganizationId = Org, Code = Unique("EVI"), Name = "Integration Evidence.pdf", Type = EvidenceType.Report,
            ControlId = "1", UploadedBy = "Tester", ExpiryDate = DateTime.Today.AddYears(1), Status = "Pending",
            FileUrl = "/evidence/test.pdf"
        });

        var fetched = await repo.GetEvidenceByIdAsync(evidence.Id, Org);
        Assert.Equal("1", fetched!.ControlId);

        fetched.Status = "Verified";
        Assert.Equal("Verified", (await repo.UpdateEvidenceAsync(fetched, Org))!.Status);

        Assert.Null(await repo.GetEvidenceByIdAsync(evidence.Id, OtherOrg));
        Assert.True(await repo.DeleteEvidenceAsync(evidence.Id, Org));
        Assert.Null(await repo.GetEvidenceByIdAsync(evidence.Id, Org));
    }

    // ── Policies & processes ────────────────────────────────────────────────────────────────────

    [DatabaseFact]
    public async Task DocumentRepository_PolicyCrud_AndProcessArchive()
    {
        var repo = new DocumentRepository(Db);

        var policy = await repo.CreatePolicyAsync(new Policy
        {
            OrganizationId = Org, Code = Unique("POL"), Title = "Integration Policy", Version = "1.0", Owner = "Tester",
            EffectiveDate = DateTime.Today, NextReviewDate = DateTime.Today.AddYears(1), Status = "Draft",
            LinkedControlIds = new() { "CTRL-001" }
        });
        var fetchedPolicy = await repo.GetPolicyByIdAsync(policy.Id, Org);
        Assert.Contains("CTRL-001", fetchedPolicy!.LinkedControlIds);

        fetchedPolicy.Version = "1.1";
        Assert.Equal("1.1", (await repo.UpdatePolicyAsync(fetchedPolicy, Org))!.Version);
        Assert.True(await repo.DeletePolicyAsync(policy.Id, Org));
        Assert.Null(await repo.GetPolicyByIdAsync(policy.Id, Org));

        var process = await repo.CreateProcessAsync(new Process
        {
            OrganizationId = Org, Code = Unique("PROC"), Title = "Integration Process", Category = "Testing",
            Owner = "Tester", Description = "desc", Steps = new() { "Step one", "Step two" }, ControlIds = new() { "CTRL-001" }
        });
        var fetchedProcess = await repo.GetProcessByIdAsync(process.Id, Org);
        Assert.Equal(2, fetchedProcess!.Steps.Count);

        Assert.True(await repo.ArchiveProcessAsync(process.Id, Org));
        Assert.NotEqual("Active", (await repo.GetProcessByIdAsync(process.Id, Org))?.Status);
    }

    // ── Management reviews & improvements ───────────────────────────────────────────────────────

    [DatabaseFact]
    public async Task ManagementReviewRepository_ReviewAndImprovementCrud()
    {
        var repo = new ManagementReviewRepository(Db);

        var review = await repo.CreateReviewAsync(new ManagementReview
        {
            OrganizationId = Org, Code = Unique("MR"), Title = "Integration Review", Period = "Q1 2027",
            ReviewDate = DateTime.Today, ChairPerson = "Tester", Attendees = new() { "A", "B" }, Summary = "s", Status = "Planned"
        });
        var fetchedReview = await repo.GetReviewByIdAsync(review.Id, Org);
        Assert.Equal(2, fetchedReview!.Attendees.Count);

        var improvement = await repo.CreateImprovementAsync(new Improvement
        {
            OrganizationId = Org, Code = Unique("IMP"), Title = "Integration Improvement", CurrentState = "now",
            FutureState = "later", Source = ImprovementSource.ManagementReview, ExpectedBenefit = "b", Owner = "Tester",
            Status = ImprovementStatus.Identified, RelatedReviewId = review.Id
        });
        var fetchedImprovement = await repo.GetImprovementByIdAsync(improvement.Id, Org);
        Assert.Equal(review.Id, fetchedImprovement!.RelatedReviewId);

        fetchedImprovement.Status = ImprovementStatus.Approved;
        Assert.Equal(ImprovementStatus.Approved, (await repo.UpdateImprovementAsync(fetchedImprovement, Org))!.Status);

        Assert.True(await repo.DeleteImprovementAsync(improvement.Id, Org));
        Assert.True(await repo.DeleteReviewAsync(review.Id, Org));
        Assert.Null(await repo.GetReviewByIdAsync(review.Id, Org));
    }

    // ── Notifications (read-only repository) ────────────────────────────────────────────────────

    [DatabaseFact]
    public async Task NotificationRepository_IsTenantScoped()
    {
        var repo = new NotificationRepository(Db);

        var all = await repo.GetNotificationsAsync(null);
        var org1 = await repo.GetNotificationsAsync(Org);

        Assert.All(org1, n => Assert.Equal(Org, n.OrganizationId));
        Assert.True(all.Count >= org1.Count);
    }

    [DatabaseFact]
    public async Task NotificationRepository_ReadStateIsPerUser_AndSourceKeysPreventDuplicates()
    {
        var repo = new NotificationRepository(Db);
        const int alex = 2, sarah = 3; // seeded users of organization 1
        var sourceKey = Unique("test-source");

        var created = await repo.CreateAsync(new Notification
        {
            OrganizationId = Org, Title = "Integration notification", Message = "m", Category = "Task", LinkUrl = "/Tasks"
        }, sourceKey);
        Assert.NotNull(created);
        Assert.Null(await repo.CreateAsync(new Notification { OrganizationId = Org, Title = "dup", Message = "m" }, sourceKey));
        var id = int.Parse(created.Id);

        var alexUnreadBefore = await repo.GetUnreadCountAsync(Org, alex);
        var sarahUnreadBefore = await repo.GetUnreadCountAsync(Org, sarah);

        // Alex reading it doesn't mark it read for Sarah.
        Assert.True(await repo.MarkReadAsync(id, Org, alex));
        Assert.Equal(alexUnreadBefore - 1, await repo.GetUnreadCountAsync(Org, alex));
        Assert.Equal(sarahUnreadBefore, await repo.GetUnreadCountAsync(Org, sarah));
        Assert.True((await repo.GetForUserAsync(Org, alex, false, 100)).Single(n => n.Id == created.Id).IsRead);
        Assert.False((await repo.GetForUserAsync(Org, sarah, false, 100)).Single(n => n.Id == created.Id).IsRead);
        Assert.DoesNotContain(await repo.GetForUserAsync(Org, alex, unreadOnly: true, 100), n => n.Id == created.Id);

        // Another tenant can't see or mark it.
        Assert.False(await repo.MarkReadAsync(id, OtherOrg, alex));
        Assert.DoesNotContain(await repo.GetForUserAsync(OtherOrg, alex, false, 100), n => n.Id == created.Id);

        Assert.True(await repo.MarkAllReadAsync(Org, sarah) >= 1);
        Assert.Equal(0, await repo.GetUnreadCountAsync(Org, sarah));
    }

    // ── Audit log (append-only) ─────────────────────────────────────────────────────────────────

    [DatabaseFact]
    public async Task AuditLogRepository_InsertAndQueryHistory()
    {
        var repo = new AuditLogRepository(Db);
        var entityId = Unique("ENT");

        await repo.InsertAsync(new AuditLog
        {
            OrganizationId = Org, ModuleName = "Tests", EntityName = "IntegrationEntity", EntityId = entityId,
            Action = AuditActionType.Update, PerformedBy = "tester@test.local", Description = "integration test",
            OldValues = "{\"name\":\"old\"}", NewValues = "{\"name\":\"new\"}", ChangedFields = "{\"name\":{\"old\":\"old\",\"new\":\"new\"}}"
        });

        var history = await repo.GetHistoryAsync("IntegrationEntity", entityId, Org);
        var entry = Assert.Single(history);
        Assert.Equal(AuditActionType.Update, entry.Action);

        var detail = await repo.GetByIdAsync(entry.Id, Org);
        Assert.NotNull(detail);
        Assert.Empty(await repo.GetHistoryAsync("IntegrationEntity", entityId, OtherOrg));

        var paged = await repo.GetPagedAsync(new AuditLogFilterDto { EntityName = "IntegrationEntity", EntityId = entityId, PageNumber = 1, PageSize = 10 }, Org);
        Assert.Equal(1, paged.TotalCount);
    }

    // ── Compliance snapshots (dashboard trend history) ──────────────────────────────────────────

    [DatabaseFact]
    public async Task ComplianceSnapshotRepository_UpsertsOnePerMonth_AndReturnsOldestFirst()
    {
        var repo = new ComplianceSnapshotRepository(Db);
        const int org = 3; // seeded org with no other snapshot activity in the tests

        await repo.UpsertMonthlySnapshotAsync(org, new DateTime(2025, 1, 15), 60.0, 9);
        await repo.UpsertMonthlySnapshotAsync(org, new DateTime(2025, 2, 10), 70.0, 7);
        await repo.UpsertMonthlySnapshotAsync(org, new DateTime(2025, 2, 25), 72.5, 6); // same month: overwrites

        var history = await repo.GetRecentSnapshotsAsync(org, 12);
        var feb = history.Single(h => h.Month == "Feb 2025");
        Assert.Equal(72.5, feb.ComplianceScore);
        Assert.Equal(6, feb.RiskScore);
        Assert.Equal(1, history.Count(h => h.Month == "Feb 2025"));
        Assert.True(history.FindIndex(h => h.Month == "Jan 2025") < history.FindIndex(h => h.Month == "Feb 2025"));

        // Platform-wide (null org) history is kept separately from tenant history.
        Assert.DoesNotContain(await repo.GetRecentSnapshotsAsync(null, 12), h => h.Month == "Jan 2025" && h.ComplianceScore == 60.0);
    }
}
