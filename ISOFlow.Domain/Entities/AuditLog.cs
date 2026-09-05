using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Entities;

/// <summary>
/// Append-only record of a meaningful business/data change. Rows are written exclusively by the
/// centralized audit pipeline (AuditActionFilter + AuditLogService) in the same database
/// transaction as the business change being recorded; there is no update/delete path.
/// </summary>
public class AuditLog
{
    public string Id { get; set; } = string.Empty;
    /// <summary>Organization this log entry belongs to. Null for platform-level (SuperAdmin) actions.</summary>
    public int? OrganizationId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditActionType Action { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public string? Description { get; set; }

    /// <summary>Raw JSONB. Populated for Update and Delete (state before the change).</summary>
    public string? OldValues { get; set; }
    /// <summary>Raw JSONB. Populated for Create and Update (state after the change).</summary>
    public string? NewValues { get; set; }
    /// <summary>Raw JSONB. Populated for Update only: {field: {old, new}} for fields that actually changed.</summary>
    public string? ChangedFields { get; set; }
}
