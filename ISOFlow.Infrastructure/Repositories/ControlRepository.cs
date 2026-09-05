using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class ControlRepository : BaseRepository, IControlRepository
{
    public ControlRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<List<Control>> GetAllControlsAsync(int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedListAsync("SELECT * FROM sp_controls_get_all(@p_organization_id)", r => new Control
        {
            Id = r.id.ToString(),
            OrganizationId = (int)r.organization_id,
            Code = (string)r.code,
            Title = (string)r.title,
            Category = (string)r.category,
            Description = (string)r.description ?? string.Empty,
            Status = (ControlStatus)(int)r.status,
            Owner = (string)r.owner,
            CompliancePercentage = (double)r.compliance_percentage,
            IsApplicable = (bool)r.is_applicable,
            Justification = (string)r.justification ?? string.Empty
        }, parameters);
    }

    public Task<PagedResponse<Control>> GetPagedControlsAsync(PagedRequestDto request, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_category", string.IsNullOrWhiteSpace(request.CategoryFilter) ? null : request.CategoryFilter.Trim());
        parameters.Add("p_status", int.TryParse(request.StatusFilter, out var st) ? st : (int?)null);

        return QueryPagedAsync(
            "SELECT * FROM sp_controls_get_paged(@p_page_number, @p_page_size, @p_organization_id, @p_search_term, @p_category, @p_status)",
            r => new Control
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                Title = (string)r.title,
                Category = (string)r.category,
                Description = (string)r.description ?? string.Empty,
                Status = (ControlStatus)(int)r.status,
                Owner = (string)r.owner,
                CompliancePercentage = (double)r.compliance_percentage,
                IsApplicable = (bool)r.is_applicable,
                Justification = (string)r.justification ?? string.Empty
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<Control?> GetControlByIdAsync(string id, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_controls_get_by_id(@id, @p_organization_id)",
            row => new Control
            {
                Id = row.id.ToString(),
                OrganizationId = (int)row.organization_id,
                Code = (string)row.code,
                Title = (string)row.title,
                Category = (string)row.category,
                Description = (string)row.description ?? string.Empty,
                Status = (ControlStatus)(int)row.status,
                Owner = (string)row.owner,
                CompliancePercentage = (double)row.compliance_percentage,
                IsApplicable = (bool)row.is_applicable,
                Justification = (string)row.justification ?? string.Empty
            },
            parameters);
    }

    public async Task<Control> CreateControlAsync(Control control)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", control.OrganizationId);
        parameters.Add("p_code", control.Code);
        parameters.Add("p_title", control.Title);
        parameters.Add("p_category", control.Category);
        parameters.Add("p_description", control.Description);
        parameters.Add("p_status", (int)control.Status);
        parameters.Add("p_owner", control.Owner);
        parameters.Add("p_compliance_percentage", control.CompliancePercentage);
        parameters.Add("p_is_applicable", control.IsApplicable);
        parameters.Add("p_justification", control.Justification);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_controls_create(@p_organization_id, @p_code, @p_title, @p_category, @p_description, @p_status, @p_owner, @p_compliance_percentage, @p_is_applicable, @p_justification)", parameters);
        control.Id = insertedId.ToString();
        return control;
    }

    public async Task<Control?> UpdateControlAsync(Control control, int organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", control.Id);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_title", control.Title);
        parameters.Add("p_category", control.Category);
        parameters.Add("p_description", control.Description);
        parameters.Add("p_status", (int)control.Status);
        parameters.Add("p_owner", control.Owner);
        parameters.Add("p_compliance_percentage", control.CompliancePercentage);
        parameters.Add("p_is_applicable", control.IsApplicable);
        parameters.Add("p_justification", control.Justification);

        var updated = await QuerySingleOrDefaultAsync<bool>("SELECT sp_controls_update(@p_id, @p_organization_id, @p_title, @p_category, @p_description, @p_status, @p_owner, @p_compliance_percentage, @p_is_applicable, @p_justification)", parameters);
        return updated ? control : null;
    }

    public async Task<bool> DeleteControlAsync(string id, int organizationId)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_controls_delete(@id, @organizationId)", new { id, organizationId });
    }

    public Task<List<StatementOfApplicability>> GetStatementOfApplicabilityAsync(int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedListAsync("SELECT * FROM sp_controls_get_soa(@p_organization_id)", r => new StatementOfApplicability
        {
            ControlId = r.control_id.ToString(),
            ControlCode = (string)r.control_code,
            ControlTitle = (string)r.control_title,
            Applicable = (bool)r.applicable,
            Justification = (string)r.justification ?? string.Empty,
            ImplementationStatus = (string)r.implementation_status,
            Owner = (string)r.owner,
            EvidenceCount = (int)(r.evidence_count ?? 0)
        }, parameters);
    }

    public async Task<RelatedItemsCountDto> GetRelatedItemsCountAsync(string controlId, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", controlId);
        parameters.Add("p_organization_id", organizationId);

        var result = await QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_controls_get_related_items_count(@p_id, @p_organization_id)",
            r => new RelatedItemsCountDto
            {
                Requirements = (int)r.requirement_count,
                Controls = 1,
                Risks = (int)r.risk_count,
                Treatments = (int)r.treatment_count,
                Policies = (int)r.policy_count,
                Processes = (int)r.process_count,
                Tasks = (int)r.task_count,
                Evidence = (int)r.evidence_count,
                Audits = (int)r.audit_count,
                Findings = (int)r.finding_count,
                Capa = (int)r.capa_count,
                Improvements = (int)r.improvement_count
            },
            parameters);

        return result ?? new RelatedItemsCountDto();
    }
}
