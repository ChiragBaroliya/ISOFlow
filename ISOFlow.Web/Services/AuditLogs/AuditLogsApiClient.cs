using ISOFlow.Application.DTOs;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.AuditLogs;

public interface IAuditLogsApiClient
{
    Task<PagedResponse<AuditLogDto>> GetPagedAsync(AuditLogFilterDto filter);
    Task<AuditLogDetailDto?> GetByIdAsync(string id);
    Task<List<AuditLogDto>> GetHistoryAsync(string entityName, string entityId);
}

public class AuditLogsApiClient : IAuditLogsApiClient
{
    private readonly IApiHttpClient _api;

    public AuditLogsApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<PagedResponse<AuditLogDto>> GetPagedAsync(AuditLogFilterDto filter)
    {
        var query = new List<string>
        {
            $"pageNumber={filter.PageNumber}",
            $"pageSize={filter.PageSize}"
        };
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm)) query.Add($"searchTerm={Uri.EscapeDataString(filter.SearchTerm)}");
        if (filter.FromDate.HasValue) query.Add($"fromDate={filter.FromDate.Value:yyyy-MM-dd}");
        if (filter.ToDate.HasValue) query.Add($"toDate={filter.ToDate.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(filter.ModuleName)) query.Add($"moduleName={Uri.EscapeDataString(filter.ModuleName)}");
        if (!string.IsNullOrWhiteSpace(filter.EntityName)) query.Add($"entityName={Uri.EscapeDataString(filter.EntityName)}");
        if (!string.IsNullOrWhiteSpace(filter.EntityId)) query.Add($"entityId={Uri.EscapeDataString(filter.EntityId)}");
        if (filter.Action.HasValue) query.Add($"action={filter.Action.Value}");
        if (!string.IsNullOrWhiteSpace(filter.PerformedBy)) query.Add($"performedBy={Uri.EscapeDataString(filter.PerformedBy)}");

        var url = "api/auditlogs?" + string.Join("&", query);
        return await _api.GetAsync<PagedResponse<AuditLogDto>>(url)
            ?? new PagedResponse<AuditLogDto>(new List<AuditLogDto>(), 0, filter.PageNumber, filter.PageSize);
    }

    public async Task<AuditLogDetailDto?> GetByIdAsync(string id) =>
        await _api.GetAsync<AuditLogDetailDto>($"api/auditlogs/{Uri.EscapeDataString(id)}");

    public async Task<List<AuditLogDto>> GetHistoryAsync(string entityName, string entityId) =>
        await _api.GetAsync<List<AuditLogDto>>($"api/auditlogs/history/{Uri.EscapeDataString(entityName)}/{Uri.EscapeDataString(entityId)}") ?? new();
}
