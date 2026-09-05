using ISOFlow.Application.Helpers;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Services;
using Xunit;

namespace ISOFlow.Tests;

/// <summary>
/// Coverage for the centralized audit pipeline's pure logic: the reflection-based diff/redaction
/// helper (<see cref="EntityDiffHelper"/>) and the action-shaping logic in <see cref="AuditLogService"/>
/// (Create/Update/Delete/business-action/no-change branches). The transactional, same-connection
/// guarantee provided by AmbientDbContext + AuditActionFilter was verified end-to-end against a real
/// Postgres instance (Create, Update, no-op Update, Delete, a business status-change action, a
/// rejected/404 write producing no audit row, and the append-only DB trigger) rather than here,
/// since this test project — like its existing RefreshTokenRepository-backed tests — has no real
/// database available in CI.
/// </summary>
public class AuditLoggingTests
{
    private sealed class SampleEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int CompliancePercentage { get; set; }
        public bool IsActive { get; set; }
        public string Password { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    #region EntityDiffHelper

    [Fact]
    public void Diff_DetectsChangedScalarFields()
    {
        var oldEntity = new SampleEntity { Id = "1", Title = "Old Title", CompliancePercentage = 10 };
        var newEntity = new SampleEntity { Id = "1", Title = "New Title", CompliancePercentage = 55 };

        var changes = EntityDiffHelper.Diff(oldEntity, newEntity);

        Assert.Equal(2, changes.Count);
        Assert.Equal(("Old Title", "New Title"), changes["Title"]);
        Assert.Equal((10, 55), changes["CompliancePercentage"]);
        Assert.False(changes.ContainsKey("Id"));
    }

    [Fact]
    public void Diff_IgnoresUnchangedFields()
    {
        var oldEntity = new SampleEntity { Id = "1", Title = "Same", CompliancePercentage = 10 };
        var newEntity = new SampleEntity { Id = "1", Title = "Same", CompliancePercentage = 10 };

        var changes = EntityDiffHelper.Diff(oldEntity, newEntity);

        Assert.Empty(changes);
    }

    [Fact]
    public void Diff_TreatsStructurallyIdenticalListsAsUnchanged()
    {
        // Two separately-constructed List<string> instances with identical contents must NOT be
        // reported as "changed" under reference equality — this was a real bug: every list-typed
        // property (very common across ISOFlow entities: LinkedControlIds, Steps, Attendees, ...)
        // would otherwise appear changed on every single Update.
        var oldEntity = new SampleEntity { Tags = new List<string> { "a", "b" } };
        var newEntity = new SampleEntity { Tags = new List<string> { "a", "b" } };

        var changes = EntityDiffHelper.Diff(oldEntity, newEntity);

        Assert.Empty(changes);
    }

    [Fact]
    public void Diff_DetectsActualListContentChanges()
    {
        var oldEntity = new SampleEntity { Tags = new List<string> { "a", "b" } };
        var newEntity = new SampleEntity { Tags = new List<string> { "a", "c" } };

        var changes = EntityDiffHelper.Diff(oldEntity, newEntity);

        Assert.True(changes.ContainsKey("Tags"));
    }

    [Fact]
    public void Diff_RedactsSensitiveFieldsEvenWhenChanged()
    {
        var oldEntity = new SampleEntity { Password = "OldSecret123" };
        var newEntity = new SampleEntity { Password = "NewSecret456" };

        var changes = EntityDiffHelper.Diff(oldEntity, newEntity);

        Assert.Equal(("***REDACTED***", "***REDACTED***"), changes["Password"]);
    }

    [Fact]
    public void ToRedactedJson_NeverIncludesRawPasswordValue()
    {
        var entity = new SampleEntity { Id = "1", Password = "TopSecret!" };

        var json = EntityDiffHelper.ToRedactedJson(entity);

        Assert.NotNull(json);
        Assert.DoesNotContain("TopSecret!", json);
        Assert.Contains("REDACTED", json);
    }

    [Fact]
    public void ToRedactedJson_NullEntity_ReturnsNull()
    {
        Assert.Null(EntityDiffHelper.ToRedactedJson(null));
    }

    #endregion

    #region AuditLogService

    private sealed class FakeAuditLogRepository : IAuditLogRepository
    {
        public AuditLog? LastInserted { get; private set; }
        public int InsertCount { get; private set; }

        public Task InsertAsync(AuditLog log)
        {
            LastInserted = log;
            InsertCount++;
            return Task.CompletedTask;
        }

        public Task<Application.DTOs.PagedResponse<Application.DTOs.AuditLogDto>> GetPagedAsync(Application.DTOs.AuditLogFilterDto filter, int? organizationId = null)
            => throw new NotImplementedException();

        public Task<Application.DTOs.AuditLogDetailDto?> GetByIdAsync(string id, int? organizationId = null) => throw new NotImplementedException();

        public Task<List<Application.DTOs.AuditLogDto>> GetHistoryAsync(string entityName, string entityId, int? organizationId = null)
            => throw new NotImplementedException();
    }

    private static AuditEntryContext BaseContext(AuditActionType action, object? oldEntity, object? newEntity) => new()
    {
        ModuleName = "Compliance",
        EntityName = "Control",
        EntityId = "CTRL-001",
        Action = action,
        OldEntity = oldEntity,
        NewEntity = newEntity,
        OrganizationId = 1,
        PerformedBy = "alex.morgan@acme.com"
    };

    [Fact]
    public async Task LogAsync_Create_StoresNewValuesOnlyAndReturnsTrue()
    {
        var repo = new FakeAuditLogRepository();
        var service = new AuditLogService(repo);
        var newEntity = new SampleEntity { Id = "1", Title = "Brand New" };

        var written = await service.LogAsync(BaseContext(AuditActionType.Create, null, newEntity));

        Assert.True(written);
        Assert.Equal(1, repo.InsertCount);
        Assert.Null(repo.LastInserted!.OldValues);
        Assert.NotNull(repo.LastInserted.NewValues);
        Assert.Null(repo.LastInserted.ChangedFields);
    }

    [Fact]
    public async Task LogAsync_Delete_StoresOldValuesOnlyAndReturnsTrue()
    {
        var repo = new FakeAuditLogRepository();
        var service = new AuditLogService(repo);
        var oldEntity = new SampleEntity { Id = "1", Title = "About To Be Deleted" };

        var written = await service.LogAsync(BaseContext(AuditActionType.Delete, oldEntity, null));

        Assert.True(written);
        Assert.NotNull(repo.LastInserted!.OldValues);
        Assert.Null(repo.LastInserted.NewValues);
        Assert.Null(repo.LastInserted.ChangedFields);
    }

    [Fact]
    public async Task LogAsync_UpdateWithRealChange_StoresOldNewAndChangedFields()
    {
        var repo = new FakeAuditLogRepository();
        var service = new AuditLogService(repo);
        var oldEntity = new SampleEntity { Id = "1", Title = "Before" };
        var newEntity = new SampleEntity { Id = "1", Title = "After" };

        var written = await service.LogAsync(BaseContext(AuditActionType.Update, oldEntity, newEntity));

        Assert.True(written);
        Assert.NotNull(repo.LastInserted!.OldValues);
        Assert.NotNull(repo.LastInserted.NewValues);
        Assert.Contains("Title", repo.LastInserted.ChangedFields);
    }

    [Fact]
    public async Task LogAsync_UpdateWithNoActualChange_WritesNothingAndReturnsFalse()
    {
        // The "No-Change" scenario: a save that round-trips identical data must not create audit noise.
        var repo = new FakeAuditLogRepository();
        var service = new AuditLogService(repo);
        var oldEntity = new SampleEntity { Id = "1", Title = "Unchanged", Tags = new List<string> { "x" } };
        var newEntity = new SampleEntity { Id = "1", Title = "Unchanged", Tags = new List<string> { "x" } };

        var written = await service.LogAsync(BaseContext(AuditActionType.Update, oldEntity, newEntity));

        Assert.False(written);
        Assert.Equal(0, repo.InsertCount);
    }

    [Fact]
    public async Task LogAsync_BusinessAction_AlwaysWritesEvenWithoutDetectableFieldDiff()
    {
        // Approve/Reject/Submit/Assign/... are meaningful because they happened, not because a
        // tracked field literally differs — unlike a plain Update, these must never be skipped.
        var repo = new FakeAuditLogRepository();
        var service = new AuditLogService(repo);
        var sameStateEntity = new SampleEntity { Id = "1", Title = "Same" };

        var written = await service.LogAsync(BaseContext(AuditActionType.Approve, sameStateEntity, sameStateEntity));

        Assert.True(written);
        Assert.Equal(1, repo.InsertCount);
    }

    #endregion
}
