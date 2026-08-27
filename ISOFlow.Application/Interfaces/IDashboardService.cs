using ISOFlow.Application.DTOs;

namespace ISOFlow.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardKpiDto> GetDashboardKpisAsync();
    Task<List<ComplianceTrendDto>> GetComplianceTrendsAsync();
    Task<List<RiskMatrixCellDto>> GetRiskMatrixAsync();
    Task<TraceabilityGraphDto> GetGoldenScenarioTraceabilityAsync();
}
