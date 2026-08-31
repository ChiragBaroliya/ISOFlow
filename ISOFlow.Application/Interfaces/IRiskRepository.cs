using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IRiskRepository
{
    Task<List<Risk>> GetAllRisksAsync();
    Task<PagedResponse<Risk>> GetPagedRisksAsync(PagedRequestDto request);
    Task<Risk?> GetRiskByIdAsync(string id);
    Task<RiskTreatment?> GetRiskTreatmentByRiskIdAsync(string riskId);
    Task<List<RiskTreatment>> GetAllRiskTreatmentsAsync();
    Task<PagedResponse<RiskTreatment>> GetPagedRiskTreatmentsAsync(PagedRequestDto request);
    Task<List<RiskMatrixCellDto>> GetRiskMatrixDataAsync();
    Task<Risk> CreateRiskAsync(Risk risk, RiskTreatment? treatment = null);
    Task<Risk?> UpdateRiskAsync(Risk risk, RiskTreatment? treatment = null);
    Task<bool> DeleteRiskAsync(string id);
}
