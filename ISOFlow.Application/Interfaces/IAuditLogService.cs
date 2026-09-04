using ISOFlow.Domain.Enums;

namespace ISOFlow.Application.Interfaces;

/// <summary>
/// Everything the centralized audit pipeline needs to describe one meaningful business change.
/// <see cref="OldEntity"/>/<see cref="NewEntity"/> are plain entity snapshots (any POCO) — the
/// service diffs and serializes them; callers never build JSON themselves.
/// </summary>
public sealed class AuditEntryContext
{
    public required string ModuleName { get; init; }
    public required string EntityName { get; init; }
    public required string EntityId { get; init; }
    public required AuditActionType Action { get; init; }

    /// <summary>State before the change. Required for Update/Delete-shaped actions.</summary>
    public object? OldEntity { get; init; }

    /// <summary>State after the change. Required for Create/Update-shaped actions.</summary>
    public object? NewEntity { get; init; }

    public string? Description { get; init; }

    public required string TenantId { get; init; }
    public required string PerformedBy { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? CorrelationId { get; init; }
}

/// <summary>
/// The single, reusable entry point for writing audit records. Controllers never call this
/// directly — it is invoked by the global <c>AuditActionFilter</c> based on the declarative
/// <c>[Audit]</c> attribute, so no per-controller audit code is ever required.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Records one audit entry. For Update actions with no actual field-level differences
    /// between <see cref="AuditEntryContext.OldEntity"/> and <see cref="AuditEntryContext.NewEntity"/>,
    /// nothing is written and this returns false (a no-op update is not a "meaningful" change).
    /// </summary>
    Task<bool> LogAsync(AuditEntryContext context);
}
