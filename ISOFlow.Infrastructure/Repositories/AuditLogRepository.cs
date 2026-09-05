using System.Text.Json;
using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

/// <inheritdoc cref="IAuditLogRepository"/>
public class AuditLogRepository : BaseRepository, IAuditLogRepository
{
    public AuditLogRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public async Task InsertAsync(AuditLog log)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", log.OrganizationId);
        parameters.Add("p_module_name", log.ModuleName);
        parameters.Add("p_entity_name", log.EntityName);
        parameters.Add("p_entity_id", log.EntityId);
        parameters.Add("p_action", log.Action.ToString());
        parameters.Add("p_performed_by", log.PerformedBy);
        parameters.Add("p_ip_address", log.IpAddress);
        parameters.Add("p_user_agent", log.UserAgent);
        parameters.Add("p_correlation_id", log.CorrelationId);
        parameters.Add("p_description", log.Description);
        parameters.Add("p_old_values", log.OldValues);
        parameters.Add("p_new_values", log.NewValues);
        parameters.Add("p_changed_fields", log.ChangedFields);

        await QuerySingleAsync<int>(
            @"SELECT sp_audit_logs_insert(
                @p_organization_id, @p_module_name, @p_entity_name, @p_entity_id, @p_action, @p_performed_by,
                @p_ip_address, @p_user_agent, @p_correlation_id, @p_description,
                @p_old_values::jsonb, @p_new_values::jsonb, @p_changed_fields::jsonb)",
            parameters);
    }

    public Task<PagedResponse<AuditLogDto>> GetPagedAsync(AuditLogFilterDto filter, int? organizationId = null)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", filter.PageNumber);
        parameters.Add("p_page_size", filter.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(filter.SearchTerm) ? null : filter.SearchTerm.Trim());
        parameters.Add("p_from_date", filter.FromDate);
        parameters.Add("p_to_date", filter.ToDate);
        parameters.Add("p_module_name", string.IsNullOrWhiteSpace(filter.ModuleName) ? null : filter.ModuleName.Trim());
        parameters.Add("p_entity_name", string.IsNullOrWhiteSpace(filter.EntityName) ? null : filter.EntityName.Trim());
        parameters.Add("p_entity_id", string.IsNullOrWhiteSpace(filter.EntityId) ? null : filter.EntityId.Trim());
        parameters.Add("p_action", filter.Action?.ToString());
        parameters.Add("p_performed_by", string.IsNullOrWhiteSpace(filter.PerformedBy) ? null : filter.PerformedBy.Trim());

        return QueryPagedAsync(
            @"SELECT * FROM sp_audit_logs_get_paged(
                @p_page_number, @p_page_size, @p_organization_id, @p_search_term, @p_from_date, @p_to_date,
                @p_module_name, @p_entity_name, @p_entity_id, @p_action, @p_performed_by)",
            MapListRow,
            parameters,
            filter.PageNumber,
            filter.PageSize);
    }

    public async Task<AuditLogDetailDto?> GetByIdAsync(string id, int? organizationId = null)
    {
        if (!int.TryParse(id, out var numericId)) return null;

        var row = await QueryFirstOrDefaultAsync<dynamic>("SELECT * FROM sp_audit_logs_get_by_id(@id, @organizationId)", new { id = numericId, organizationId });
        if (row == null) return null;

        var action = ParseAction((string)row.action);
        string? oldJson = row.old_values;
        string? newJson = row.new_values;
        string? changedJson = row.changed_fields;

        return new AuditLogDetailDto
        {
            Id = row.id.ToString(),
            OrganizationId = row.organization_id == null ? (int?)null : (int)row.organization_id,
            ModuleName = (string)row.module_name,
            EntityName = (string)row.entity_name,
            EntityId = (string)row.entity_id,
            Action = action,
            PerformedBy = (string)row.performed_by,
            PerformedAt = (DateTime)row.performed_at,
            IpAddress = (string?)row.ip_address,
            UserAgent = (string?)row.user_agent,
            CorrelationId = (string?)row.correlation_id,
            Description = (string?)row.description,
            ChangedFields = BuildFieldChangeList(action, oldJson, newJson, changedJson)
        };
    }

    public Task<List<AuditLogDto>> GetHistoryAsync(string entityName, string entityId, int? organizationId = null)
    {
        return QueryMappedListAsync(
            "SELECT * FROM sp_audit_logs_get_history(@entityName, @entityId, @organizationId)",
            MapListRow,
            new { entityName, entityId, organizationId });
    }

    private static AuditLogDto MapListRow(dynamic r) => new()
    {
        Id = r.id.ToString(),
        ModuleName = (string)r.module_name,
        EntityName = (string)r.entity_name,
        EntityId = (string)r.entity_id,
        Action = ParseAction((string)r.action),
        PerformedBy = (string)r.performed_by,
        PerformedAt = (DateTime)r.performed_at,
        Description = (string?)r.description
    };

    private static AuditActionType ParseAction(string raw) =>
        Enum.TryParse<AuditActionType>(raw, out var action) ? action : AuditActionType.Update;

    /// <summary>
    /// Shapes the stored JSONB payloads into a flat Field/OldValue/NewValue list for the Audit Detail view.
    /// Prefers the stored ChangedFields diff (Update); falls back to the full OldValues (Delete) or
    /// NewValues (Create/other) snapshot when no diff was stored.
    /// </summary>
    private static List<AuditLogFieldChangeDto> BuildFieldChangeList(AuditActionType action, string? oldJson, string? newJson, string? changedJson)
    {
        var result = new List<AuditLogFieldChangeDto>();

        if (!string.IsNullOrWhiteSpace(changedJson))
        {
            using var doc = JsonDocument.Parse(changedJson);
            foreach (var field in doc.RootElement.EnumerateObject())
            {
                var oldValue = field.Value.TryGetProperty("old", out var o) ? FormatJsonValue(o) : null;
                var newValue = field.Value.TryGetProperty("new", out var n) ? FormatJsonValue(n) : null;
                result.Add(new AuditLogFieldChangeDto { Field = field.Name, OldValue = oldValue, NewValue = newValue });
            }
            return result;
        }

        if (action == AuditActionType.Delete && !string.IsNullOrWhiteSpace(oldJson))
        {
            using var doc = JsonDocument.Parse(oldJson);
            foreach (var field in doc.RootElement.EnumerateObject())
            {
                result.Add(new AuditLogFieldChangeDto { Field = field.Name, OldValue = FormatJsonValue(field.Value), NewValue = null });
            }
            return result;
        }

        if (!string.IsNullOrWhiteSpace(newJson))
        {
            using var doc = JsonDocument.Parse(newJson);
            foreach (var field in doc.RootElement.EnumerateObject())
            {
                result.Add(new AuditLogFieldChangeDto { Field = field.Name, OldValue = null, NewValue = FormatJsonValue(field.Value) });
            }
        }

        return result;
    }

    private static string? FormatJsonValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        JsonValueKind.String => element.GetString(),
        JsonValueKind.True or JsonValueKind.False => element.GetBoolean().ToString(),
        _ => element.GetRawText()
    };
}
