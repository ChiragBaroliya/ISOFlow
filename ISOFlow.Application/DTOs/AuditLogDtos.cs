using ISOFlow.Domain.Enums;

namespace ISOFlow.Application.DTOs;

/// <summary>Query filters for the Audit Logs browser (server-side paginated).</summary>
public class AuditLogFilterDto : PagedRequestDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? ModuleName { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public AuditActionType? Action { get; set; }
    public string? PerformedBy { get; set; }
}

/// <summary>Single row in the Audit Logs list / an entity's History tab.</summary>
public class AuditLogDto
{
    public string Id { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditActionType Action { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; }
    public string? Description { get; set; }
}

/// <summary>One field's before/after value, for the Audit Detail "Field | Old Value | New Value" view.</summary>
public class AuditLogFieldChangeDto
{
    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

/// <summary>Full detail of a single audit entry, including the field-level diff.</summary>
public class AuditLogDetailDto : AuditLogDto
{
    public int? OrganizationId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public List<AuditLogFieldChangeDto> ChangedFields { get; set; } = new();
}
