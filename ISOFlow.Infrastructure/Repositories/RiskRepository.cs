using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.MockData;

namespace ISOFlow.Infrastructure.Repositories;

public class RiskRepository : IRiskRepository
{
    public Task<List<Risk>> GetAllRisksAsync() => Task.FromResult(MockStore.Risks);

    public Task<Risk?> GetRiskByIdAsync(string id) =>
        Task.FromResult(MockStore.Risks.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || r.Code.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<RiskTreatment?> GetRiskTreatmentByRiskIdAsync(string riskId) =>
        Task.FromResult(MockStore.RiskTreatments.FirstOrDefault(t => t.RiskId.Equals(riskId, StringComparison.OrdinalIgnoreCase)));

    public Task<List<RiskTreatment>> GetAllRiskTreatmentsAsync() => Task.FromResult(MockStore.RiskTreatments);

    public Task<List<RiskMatrixCellDto>> GetRiskMatrixDataAsync()
    {
        var matrix = new List<RiskMatrixCellDto>();
        for (int l = 1; l <= 5; l++)
        {
            for (int i = 1; i <= 5; i++)
            {
                var matchingRisks = MockStore.Risks.Where(r => (int)r.Likelihood == l && (int)r.Impact == i).ToList();
                matrix.Add(new RiskMatrixCellDto
                {
                    Likelihood = l,
                    Impact = i,
                    Count = matchingRisks.Count,
                    RiskCodes = matchingRisks.Select(r => r.Code).ToList()
                });
            }
        }
        return Task.FromResult(matrix);
    }

    public Task<Risk> CreateRiskAsync(Risk risk, RiskTreatment? treatment = null)
    {
        if (string.IsNullOrWhiteSpace(risk.Id))
        {
            risk.Id = MockStore.NextId("RISK", MockStore.Risks.Count);
        }
        if (string.IsNullOrWhiteSpace(risk.Code))
        {
            risk.Code = risk.Id;
        }

        if (treatment != null && !string.IsNullOrWhiteSpace(treatment.TreatmentPlan))
        {
            treatment.Id = MockStore.NextId("TRT", MockStore.RiskTreatments.Count);
            treatment.RiskId = risk.Id;
            risk.TreatmentId = treatment.Id;
            MockStore.RiskTreatments.Add(treatment);
        }

        MockStore.Risks.Add(risk);
        return Task.FromResult(risk);
    }

    public Task<Risk?> UpdateRiskAsync(Risk risk, RiskTreatment? treatment = null)
    {
        var existing = MockStore.Risks.FirstOrDefault(r => r.Id.Equals(risk.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Title = risk.Title;
            existing.Description = risk.Description;
            existing.Asset = risk.Asset;
            existing.Department = risk.Department;
            existing.Owner = risk.Owner;
            existing.Likelihood = risk.Likelihood;
            existing.Impact = risk.Impact;
            existing.Status = risk.Status;
            existing.ControlId = risk.ControlId;

            if (treatment != null)
            {
                var existingTreatment = MockStore.RiskTreatments.FirstOrDefault(t => t.RiskId.Equals(risk.Id, StringComparison.OrdinalIgnoreCase));
                if (existingTreatment != null)
                {
                    existingTreatment.Option = treatment.Option;
                    existingTreatment.TreatmentPlan = treatment.TreatmentPlan;
                    existingTreatment.Owner = treatment.Owner;
                    existingTreatment.TargetDate = treatment.TargetDate;
                    existingTreatment.ResidualLikelihood = treatment.ResidualLikelihood;
                    existingTreatment.ResidualImpact = treatment.ResidualImpact;
                    existingTreatment.Status = treatment.Status;
                }
                else if (!string.IsNullOrWhiteSpace(treatment.TreatmentPlan))
                {
                    treatment.Id = MockStore.NextId("TRT", MockStore.RiskTreatments.Count);
                    treatment.RiskId = risk.Id;
                    existing.TreatmentId = treatment.Id;
                    MockStore.RiskTreatments.Add(treatment);
                }
            }
        }
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteRiskAsync(string id)
    {
        var existing = MockStore.Risks.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            MockStore.Risks.Remove(existing);
            MockStore.RiskTreatments.RemoveAll(t => t.RiskId.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
