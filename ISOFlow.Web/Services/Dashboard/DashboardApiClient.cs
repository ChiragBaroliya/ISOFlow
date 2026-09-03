using ISOFlow.Application.DTOs;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Dashboard;

public interface IDashboardApiClient
{
    Task<DashboardKpiDto> GetDashboardKpisAsync();
    Task<List<ComplianceTrendDto>> GetComplianceTrendsAsync();
    Task<List<RiskMatrixCellDto>> GetRiskMatrixAsync();
    Task<TraceabilityGraphDto?> GetTraceabilityGraphAsync();
}

public class DashboardApiClient : IDashboardApiClient
{
    private readonly IApiHttpClient _api;

    public DashboardApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<DashboardKpiDto> GetDashboardKpisAsync() =>
        await _api.GetAsync<DashboardKpiDto>("api/dashboard/kpis") ?? new DashboardKpiDto();

    public async Task<List<ComplianceTrendDto>> GetComplianceTrendsAsync() =>
        await _api.GetAsync<List<ComplianceTrendDto>>("api/dashboard/trends") ?? new();

    public async Task<List<RiskMatrixCellDto>> GetRiskMatrixAsync() =>
        await _api.GetAsync<List<RiskMatrixCellDto>>("api/dashboard/risk-matrix") ?? new();

    public async Task<TraceabilityGraphDto?> GetTraceabilityGraphAsync() =>
        await _api.GetAsync<TraceabilityGraphDto>("api/dashboard/traceability");
}
