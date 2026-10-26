using ISOFlow.Application.DTOs;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Logs;

public interface ILogsApiClient
{
    Task<List<LogEntryDto>> GetLogsAsync(DateTime? fromDate, DateTime? toDate, string? logLevel, string? search);
}

public class LogsApiClient : ILogsApiClient
{
    private readonly IApiHttpClient _api;

    public LogsApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<LogEntryDto>> GetLogsAsync(DateTime? fromDate, DateTime? toDate, string? logLevel, string? search)
    {
        var query = new List<string>();
        if (fromDate.HasValue) query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) query.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(logLevel) && logLevel != "All") query.Add($"logLevel={Uri.EscapeDataString(logLevel)}");
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");

        var url = query.Count > 0 ? "api/logs?" + string.Join("&", query) : "api/logs";
        return await _api.GetAsync<List<LogEntryDto>>(url) ?? new();
    }
}
