using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IRiskRepository
{
    Task<List<Risk>> GetAllRisksAsync();
    Task<Risk?> GetRiskByIdAsync(string id);
    Task<RiskTreatment?> GetRiskTreatmentByRiskIdAsync(string riskId);
    Task<List<RiskTreatment>> GetAllRiskTreatmentsAsync();
    Task<List<RiskMatrixCellDto>> GetRiskMatrixDataAsync();
    Task<Risk> CreateRiskAsync(Risk risk, RiskTreatment? treatment = null);
    Task<Risk?> UpdateRiskAsync(Risk risk, RiskTreatment? treatment = null);
    Task<bool> DeleteRiskAsync(string id);
}
