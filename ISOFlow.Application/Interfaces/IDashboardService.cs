using ISOFlow.Application.DTOs;

namespace ISOFlow.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardKpiDto> GetDashboardKpisAsync(int? organizationId);
    Task<List<ComplianceTrendDto>> GetComplianceTrendsAsync(int? organizationId);
    Task<List<RiskMatrixCellDto>> GetRiskMatrixAsync(int? organizationId);
    Task<TraceabilityGraphDto> GetControlTraceabilityAsync(string controlId, int? organizationId);
}
