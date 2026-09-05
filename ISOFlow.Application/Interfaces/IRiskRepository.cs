using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IRiskRepository
{
    Task<List<Risk>> GetAllRisksAsync(int? organizationId);
    Task<PagedResponse<Risk>> GetPagedRisksAsync(PagedRequestDto request, int? organizationId);
    Task<Risk?> GetRiskByIdAsync(string id, int? organizationId);
    Task<RiskTreatment?> GetRiskTreatmentByRiskIdAsync(string riskId, int? organizationId);
    Task<List<RiskTreatment>> GetAllRiskTreatmentsAsync(int? organizationId);
    Task<PagedResponse<RiskTreatment>> GetPagedRiskTreatmentsAsync(PagedRequestDto request, int? organizationId);
    Task<List<RiskMatrixCellDto>> GetRiskMatrixDataAsync(int? organizationId);
    Task<Risk> CreateRiskAsync(Risk risk, RiskTreatment? treatment = null);
    Task<Risk?> UpdateRiskAsync(Risk risk, int organizationId, RiskTreatment? treatment = null);
    Task<bool> DeleteRiskAsync(string id, int organizationId);
}
