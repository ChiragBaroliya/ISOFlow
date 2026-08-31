using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class RiskRepository : BaseRepository, IRiskRepository
{
    public RiskRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<List<Risk>> GetAllRisksAsync()
    {
        return QueryMappedListAsync("SELECT * FROM sp_risks_get_all()", r => new Risk
        {
            Id = r.id.ToString(),
            Code = (string)r.code,
            Title = (string)r.title,
            Description = (string)r.description ?? string.Empty,
            Asset = (string)r.asset,
            Department = (string)r.department,
            Owner = (string)r.owner,
            Likelihood = (RiskLikelihood)(int)r.likelihood,
            Impact = (RiskImpact)(int)r.impact,
            TreatmentId = r.treatment_id != null ? r.treatment_id.ToString() : string.Empty,
            ControlId = r.control_id != null ? r.control_id.ToString() : string.Empty,
            Status = (string)r.status
        });
    }

    public Task<PagedResponse<Risk>> GetPagedRisksAsync(PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", string.IsNullOrWhiteSpace(request.StatusFilter) ? null : request.StatusFilter.Trim());

        return QueryPagedAsync(
            "SELECT * FROM sp_risks_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_status)",
            r => new Risk
            {
                Id = r.id.ToString(),
                Code = (string)r.code,
                Title = (string)r.title,
                Description = (string)r.description ?? string.Empty,
                Asset = (string)r.asset,
                Department = (string)r.department,
                Owner = (string)r.owner,
                Likelihood = (RiskLikelihood)(int)r.likelihood,
                Impact = (RiskImpact)(int)r.impact,
                TreatmentId = r.treatment_id != null ? r.treatment_id.ToString() : string.Empty,
                ControlId = r.control_id != null ? r.control_id.ToString() : string.Empty,
                Status = (string)r.status
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<Risk?> GetRiskByIdAsync(string id)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_risks_get_by_id(@id)",
            r => new Risk
            {
                Id = r.id.ToString(),
                Code = (string)r.code,
                Title = (string)r.title,
                Description = (string)r.description ?? string.Empty,
                Asset = (string)r.asset,
                Department = (string)r.department,
                Owner = (string)r.owner,
                Likelihood = (RiskLikelihood)(int)r.likelihood,
                Impact = (RiskImpact)(int)r.impact,
                TreatmentId = r.treatment_id != null ? r.treatment_id.ToString() : string.Empty,
                ControlId = r.control_id != null ? r.control_id.ToString() : string.Empty,
                Status = (string)r.status
            },
            new { id });
    }

    public async Task<Risk> CreateRiskAsync(Risk risk, RiskTreatment? treatment = null)
    {
        int.TryParse(risk.ControlId, out var ctrlId);
        var parameters = new DynamicParameters();
        parameters.Add("p_code", risk.Code);
        parameters.Add("p_title", risk.Title);
        parameters.Add("p_description", risk.Description);
        parameters.Add("p_asset", risk.Asset);
        parameters.Add("p_department", risk.Department);
        parameters.Add("p_owner", risk.Owner);
        parameters.Add("p_likelihood", (int)risk.Likelihood);
        parameters.Add("p_impact", (int)risk.Impact);
        parameters.Add("p_control_id", ctrlId > 0 ? (int?)ctrlId : null);
        parameters.Add("p_status", risk.Status);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_risks_create(@p_code, @p_title, @p_description, @p_asset, @p_department, @p_owner, @p_likelihood, @p_impact, @p_control_id, @p_status)", parameters);
        risk.Id = insertedId.ToString();

        if (treatment != null)
        {
            var trtParams = new DynamicParameters();
            trtParams.Add("p_risk_id", insertedId);
            trtParams.Add("p_option", treatment.Option);
            trtParams.Add("p_treatment_plan", treatment.TreatmentPlan);
            trtParams.Add("p_owner", treatment.Owner);
            trtParams.Add("p_target_date", treatment.TargetDate);
            trtParams.Add("p_residual_likelihood", (int)treatment.ResidualLikelihood);
            trtParams.Add("p_residual_impact", (int)treatment.ResidualImpact);
            trtParams.Add("p_status", treatment.Status);

            var trtId = await QuerySingleAsync<int>("SELECT sp_risk_treatments_create(@p_risk_id, @p_option, @p_treatment_plan, @p_owner, @p_target_date, @p_residual_likelihood, @p_residual_impact, @p_status)", trtParams);
            treatment.Id = trtId.ToString();
            treatment.RiskId = risk.Id;
            risk.TreatmentId = treatment.Id;
        }

        return risk;
    }

    public async Task<Risk?> UpdateRiskAsync(Risk risk, RiskTreatment? treatment = null)
    {
        int.TryParse(risk.ControlId, out var ctrlId);
        var parameters = new DynamicParameters();
        parameters.Add("p_id", risk.Id);
        parameters.Add("p_title", risk.Title);
        parameters.Add("p_description", risk.Description);
        parameters.Add("p_asset", risk.Asset);
        parameters.Add("p_department", risk.Department);
        parameters.Add("p_owner", risk.Owner);
        parameters.Add("p_likelihood", (int)risk.Likelihood);
        parameters.Add("p_impact", (int)risk.Impact);
        parameters.Add("p_control_id", ctrlId > 0 ? (int?)ctrlId : null);
        parameters.Add("p_status", risk.Status);

        var updated = await QuerySingleOrDefaultAsync<bool>("SELECT sp_risks_update(@p_id, @p_title, @p_description, @p_asset, @p_department, @p_owner, @p_likelihood, @p_impact, @p_control_id, @p_status)", parameters);
        return updated ? risk : null;
    }

    public async Task<bool> DeleteRiskAsync(string id)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_risks_delete(@id)", new { id });
    }

    public Task<RiskTreatment?> GetRiskTreatmentByRiskIdAsync(string riskId)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_risk_treatments_get_by_risk_id(@riskId)",
            t => new RiskTreatment
            {
                Id = t.id.ToString(),
                RiskId = t.risk_id.ToString(),
                Option = (string)t.option,
                TreatmentPlan = (string)t.treatment_plan,
                Owner = (string)t.owner,
                TargetDate = (DateTime)t.target_date,
                ResidualLikelihood = (RiskLikelihood)(int)t.residual_likelihood,
                ResidualImpact = (RiskImpact)(int)t.residual_impact,
                Status = (string)t.status
            },
            new { riskId });
    }

    public Task<List<RiskTreatment>> GetAllRiskTreatmentsAsync()
    {
        return QueryMappedListAsync(
            "SELECT id, risk_id, option, treatment_plan, owner, target_date, residual_likelihood, residual_impact, status FROM risk_treatments ORDER BY id ASC",
            t => new RiskTreatment
            {
                Id = t.id.ToString(),
                RiskId = t.risk_id.ToString(),
                Option = (string)t.option,
                TreatmentPlan = (string)t.treatment_plan,
                Owner = (string)t.owner,
                TargetDate = (DateTime)t.target_date,
                ResidualLikelihood = (RiskLikelihood)(int)t.residual_likelihood,
                ResidualImpact = (RiskImpact)(int)t.residual_impact,
                Status = (string)t.status
            });
    }

    public Task<PagedResponse<RiskTreatment>> GetPagedRiskTreatmentsAsync(PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);

        return QueryPagedAsync(
            "SELECT * FROM sp_risk_treatments_get_paged(@p_page_number, @p_page_size)",
            t => new RiskTreatment
            {
                Id = t.id.ToString(),
                RiskId = t.risk_id.ToString(),
                Option = (string)t.option,
                TreatmentPlan = (string)t.treatment_plan,
                Owner = (string)t.owner,
                TargetDate = (DateTime)t.target_date,
                ResidualLikelihood = (RiskLikelihood)(int)t.residual_likelihood,
                ResidualImpact = (RiskImpact)(int)t.residual_impact,
                Status = (string)t.status
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<List<RiskMatrixCellDto>> GetRiskMatrixDataAsync()
    {
        return QueryMappedListAsync("SELECT * FROM sp_get_risk_heatmap_matrix()", r => new RiskMatrixCellDto
        {
            Likelihood = (int)r.likelihood,
            Impact = (int)r.impact,
            Count = (int)(r.risk_count ?? 0),
            RiskCodes = ((string)r.risk_ids ?? string.Empty).Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList()
        });
    }
}
