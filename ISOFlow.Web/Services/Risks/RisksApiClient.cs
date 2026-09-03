using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Risks;

public interface IRisksApiClient
{
    Task<List<Risk>> GetAllRisksAsync();
    Task<Risk?> GetRiskByIdAsync(string id);
    Task<Risk?> CreateRiskAsync(Risk risk, RiskTreatment? treatment);
    Task<Risk?> UpdateRiskAsync(Risk risk, RiskTreatment? treatment);
    Task<bool> DeleteRiskAsync(string id);
    Task<RiskTreatment?> GetRiskTreatmentByRiskIdAsync(string riskId);
    Task<List<RiskMatrixCellDto>> GetRiskMatrixDataAsync();
}

public class RisksApiClient : IRisksApiClient
{
    private readonly IApiHttpClient _api;

    public RisksApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<Risk>> GetAllRisksAsync() =>
        await _api.GetAsync<List<Risk>>("api/risks/all") ?? new();

    public async Task<Risk?> GetRiskByIdAsync(string id) =>
        await _api.GetAsync<Risk>($"api/risks/{Uri.EscapeDataString(id)}");

    public async Task<Risk?> CreateRiskAsync(Risk risk, RiskTreatment? treatment) =>
        await _api.PostAsync<Risk>("api/risks", new
        {
            risk.Code, risk.Title, risk.Description, risk.Asset,
            risk.Department, risk.Owner, risk.Likelihood, risk.Impact,
            risk.ControlId, risk.Status,
            Treatment = treatment != null ? new
            {
                treatment.Option, treatment.TreatmentPlan, treatment.Owner,
                treatment.TargetDate, treatment.ResidualLikelihood,
                treatment.ResidualImpact, treatment.Status
            } : null
        });

    public async Task<Risk?> UpdateRiskAsync(Risk risk, RiskTreatment? treatment) =>
        await _api.PutAsync<Risk>($"api/risks/{Uri.EscapeDataString(risk.Id)}", new
        {
            risk.Code, risk.Title, risk.Description, risk.Asset,
            risk.Department, risk.Owner, risk.Likelihood, risk.Impact,
            risk.ControlId, risk.Status,
            Treatment = treatment != null ? new
            {
                treatment.Option, treatment.TreatmentPlan, treatment.Owner,
                treatment.TargetDate, treatment.ResidualLikelihood,
                treatment.ResidualImpact, treatment.Status
            } : null
        });

    public async Task<bool> DeleteRiskAsync(string id) =>
        await _api.DeleteAsync($"api/risks/{Uri.EscapeDataString(id)}");

    public async Task<RiskTreatment?> GetRiskTreatmentByRiskIdAsync(string riskId) =>
        await _api.GetAsync<RiskTreatment>($"api/risks/{Uri.EscapeDataString(riskId)}/treatment");

    public async Task<List<RiskMatrixCellDto>> GetRiskMatrixDataAsync() =>
        await _api.GetAsync<List<RiskMatrixCellDto>>("api/risks/matrix") ?? new();
}
