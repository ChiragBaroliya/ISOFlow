using ISOFlow.Domain.Enums;

namespace ISOFlow.Api.Auditing;

/// <summary>
/// Declares that an action creates a meaningful business/data change that must be recorded in the
/// centralized audit trail. This is the ONLY thing a controller action needs — the global
/// <see cref="AuditActionFilter"/> does everything else (snapshotting, diffing, writing, sharing
/// the business operation's own database transaction). No controller ever calls the audit
/// service directly.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AuditAttribute : Attribute
{
    /// <summary>Functional area shown in the Audit Logs "Module" column/filter, e.g. "Compliance", "Risk", "Users".</summary>
    public required string Module { get; init; }

    /// <summary>
    /// Logical entity name, e.g. "Control", "Risk", "Task". Must match the key used to register a
    /// snapshot resolver in <see cref="IAuditSnapshotRegistry"/> when <see cref="Action"/> needs a
    /// before/after snapshot (i.e. anything other than a bare Create with no lookup).
    /// </summary>
    public required string Entity { get; init; }

    public required AuditActionType Action { get; init; }

    /// <summary>Name of the route/query parameter carrying the entity id. Defaults to "id".</summary>
    public string IdParameter { get; init; } = "id";

    /// <summary>
    /// Whether to snapshot entity state before the action runs. Defaults to true for every action
    /// except Create (which has nothing to snapshot yet).
    /// </summary>
    public bool CaptureOldValue => Action != AuditActionType.Create;

    /// <summary>Whether to snapshot entity state after the action runs. False only for Delete.</summary>
    public bool CaptureNewValue => Action != AuditActionType.Delete;
}
