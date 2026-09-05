using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class AuditRepository : BaseRepository, IAuditRepository
{
    public AuditRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<List<AuditProgram>> GetAuditProgramsAsync()
    {
        return QueryMappedListAsync(
            "SELECT id, title, year, scope, status FROM audit_programs ORDER BY year DESC",
            r => new AuditProgram
            {
                Id = r.id.ToString(),
                Title = (string)r.title,
                Year = (int)r.year,
                Scope = (string)r.scope ?? string.Empty,
                Status = (string)r.status
            });
    }

    public async Task<PagedResponse<AuditProgram>> GetPagedAuditProgramsAsync(PagedRequestDto request)
    {
        var list = await GetAuditProgramsAsync();
        var total = list.Count;
        var paged = list.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();
        return new PagedResponse<AuditProgram>(paged, total, request.PageNumber, request.PageSize);
    }

    public Task<List<Audit>> GetAllAuditsAsync(int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedListAsync("SELECT * FROM sp_audits_get_all(@p_organization_id)", r => new Audit
        {
            Id = r.id.ToString(),
            OrganizationId = (int)r.organization_id,
            Code = (string)r.code,
            Title = (string)r.title,
            StandardId = r.standard_id != null ? r.standard_id.ToString() : string.Empty,
            LeadAuditor = (string)r.lead_auditor,
            StartDate = (DateTime)r.start_date,
            EndDate = (DateTime)r.end_date,
            Status = (AuditStatus)(int)r.status,
            CompletionPercentage = (int)r.completion_percentage,
            Scope = (string)r.scope ?? string.Empty
        }, parameters);
    }

    public Task<PagedResponse<Audit>> GetPagedAuditsAsync(PagedRequestDto request, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", int.TryParse(request.StatusFilter, out var st) ? st : (int?)null);

        return QueryPagedAsync(
            "SELECT * FROM sp_audits_get_paged(@p_page_number, @p_page_size, @p_organization_id, @p_search_term, @p_status)",
            r => new Audit
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                Title = (string)r.title,
                StandardId = r.standard_id != null ? r.standard_id.ToString() : string.Empty,
                LeadAuditor = (string)r.lead_auditor,
                StartDate = (DateTime)r.start_date,
                EndDate = (DateTime)r.end_date,
                Status = (AuditStatus)(int)r.status,
                CompletionPercentage = (int)r.completion_percentage,
                Scope = (string)r.scope ?? string.Empty
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<Audit?> GetAuditByIdAsync(string id, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_audits_get_by_id(@id, @p_organization_id)",
            r => new Audit
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                Title = (string)r.title,
                StandardId = r.standard_id != null ? r.standard_id.ToString() : string.Empty,
                LeadAuditor = (string)r.lead_auditor,
                StartDate = (DateTime)r.start_date,
                EndDate = (DateTime)r.end_date,
                Status = (AuditStatus)(int)r.status,
                CompletionPercentage = (int)r.completion_percentage,
                Scope = (string)r.scope ?? string.Empty
            },
            parameters);
    }

    public async Task<Audit> CreateAuditAsync(Audit audit)
    {
        int.TryParse(audit.StandardId, out var stdId);
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", audit.OrganizationId);
        parameters.Add("p_code", audit.Code);
        parameters.Add("p_title", audit.Title);
        parameters.Add("p_standard_id", stdId > 0 ? (int?)stdId : null);
        parameters.Add("p_lead_auditor", audit.LeadAuditor);
        parameters.Add("p_start_date", audit.StartDate);
        parameters.Add("p_end_date", audit.EndDate);
        parameters.Add("p_status", (int)audit.Status);
        parameters.Add("p_completion_percentage", audit.CompletionPercentage);
        parameters.Add("p_scope", audit.Scope);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_audits_create(@p_organization_id, @p_code, @p_title, @p_standard_id, @p_lead_auditor, @p_start_date, @p_end_date, @p_status, @p_completion_percentage, @p_scope)", parameters);
        audit.Id = insertedId.ToString();
        return audit;
    }

    public async Task<Audit?> UpdateAuditAsync(Audit audit, int organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", audit.Id);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_title", audit.Title);
        parameters.Add("p_lead_auditor", audit.LeadAuditor);
        parameters.Add("p_start_date", audit.StartDate);
        parameters.Add("p_end_date", audit.EndDate);
        parameters.Add("p_status", (int)audit.Status);
        parameters.Add("p_completion_percentage", audit.CompletionPercentage);
        parameters.Add("p_scope", audit.Scope);

        var updated = await QuerySingleOrDefaultAsync<bool>("SELECT sp_audits_update(@p_id, @p_organization_id, @p_title, @p_lead_auditor, @p_start_date, @p_end_date, @p_status, @p_completion_percentage, @p_scope)", parameters);
        return updated ? audit : null;
    }

    public async Task<bool> DeleteAuditAsync(string id, int organizationId)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_audits_delete(@id, @organizationId)", new { id, organizationId });
    }

    public Task<List<Finding>> GetAllFindingsAsync(int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedListAsync(
            "SELECT * FROM sp_findings_get_all(@p_organization_id)",
            f => new Finding
            {
                Id = f.id.ToString(),
                OrganizationId = (int)f.organization_id,
                Code = (string)f.code,
                Title = (string)f.title,
                AuditId = f.audit_id != null ? f.audit_id.ToString() : string.Empty,
                RequirementId = f.requirement_id != null ? f.requirement_id.ToString() : string.Empty,
                ControlId = f.control_id != null ? f.control_id.ToString() : string.Empty,
                Severity = (FindingSeverity)(int)f.severity,
                Status = (FindingStatus)(int)f.status,
                Description = (string)f.description ?? string.Empty,
                RootCause = (string)f.root_cause ?? string.Empty,
                IdentifiedDate = (DateTime)f.identified_date,
                Auditor = (string)f.auditor,
                CapaId = f.capa_id != null ? f.capa_id.ToString() : string.Empty
            },
            parameters);
    }

    public Task<PagedResponse<Finding>> GetPagedFindingsAsync(PagedRequestDto request, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", int.TryParse(request.StatusFilter, out var st) ? st : (int?)null);

        return QueryPagedAsync(
            "SELECT * FROM sp_findings_get_paged(@p_page_number, @p_page_size, @p_organization_id, @p_search_term, @p_status)",
            f => new Finding
            {
                Id = f.id.ToString(),
                OrganizationId = (int)f.organization_id,
                Code = (string)f.code,
                Title = (string)f.title,
                AuditId = f.audit_id != null ? f.audit_id.ToString() : string.Empty,
                RequirementId = f.requirement_id != null ? f.requirement_id.ToString() : string.Empty,
                ControlId = f.control_id != null ? f.control_id.ToString() : string.Empty,
                Severity = (FindingSeverity)(int)f.severity,
                Status = (FindingStatus)(int)f.status,
                Description = (string)f.description ?? string.Empty,
                RootCause = (string)f.root_cause ?? string.Empty,
                IdentifiedDate = (DateTime)f.identified_date,
                Auditor = (string)f.auditor,
                CapaId = f.capa_id != null ? f.capa_id.ToString() : string.Empty
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<Finding?> GetFindingByIdAsync(string id, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_findings_get_by_id(@id, @p_organization_id)",
            f => new Finding
            {
                Id = f.id.ToString(),
                OrganizationId = (int)f.organization_id,
                Code = (string)f.code,
                Title = (string)f.title,
                AuditId = f.audit_id != null ? f.audit_id.ToString() : string.Empty,
                RequirementId = f.requirement_id != null ? f.requirement_id.ToString() : string.Empty,
                ControlId = f.control_id != null ? f.control_id.ToString() : string.Empty,
                Severity = (FindingSeverity)(int)f.severity,
                Status = (FindingStatus)(int)f.status,
                Description = (string)f.description ?? string.Empty,
                RootCause = (string)f.root_cause ?? string.Empty,
                IdentifiedDate = (DateTime)f.identified_date,
                Auditor = (string)f.auditor,
                CapaId = f.capa_id != null ? f.capa_id.ToString() : string.Empty
            },
            parameters);
    }

    public async Task<Finding> CreateFindingAsync(Finding finding)
    {
        int.TryParse(finding.AuditId, out var auditId);
        int.TryParse(finding.RequirementId, out var reqId);
        int.TryParse(finding.ControlId, out var ctrlId);

        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", finding.OrganizationId);
        parameters.Add("p_code", finding.Code);
        parameters.Add("p_title", finding.Title);
        parameters.Add("p_audit_id", auditId > 0 ? (int?)auditId : null);
        parameters.Add("p_requirement_id", reqId > 0 ? (int?)reqId : null);
        parameters.Add("p_control_id", ctrlId > 0 ? (int?)ctrlId : null);
        parameters.Add("p_severity", (int)finding.Severity);
        parameters.Add("p_status", (int)finding.Status);
        parameters.Add("p_description", finding.Description);
        parameters.Add("p_root_cause", finding.RootCause);
        parameters.Add("p_auditor", finding.Auditor);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_findings_create(@p_organization_id, @p_code, @p_title, @p_audit_id, @p_requirement_id, @p_control_id, @p_severity, @p_status, @p_description, @p_root_cause, @p_auditor)", parameters);
        finding.Id = insertedId.ToString();
        return finding;
    }

    public async Task<Finding?> UpdateFindingAsync(Finding finding, int organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", finding.Id);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_title", finding.Title);
        parameters.Add("p_severity", (int)finding.Severity);
        parameters.Add("p_status", (int)finding.Status);
        parameters.Add("p_description", finding.Description);
        parameters.Add("p_root_cause", finding.RootCause);

        var updated = await QuerySingleOrDefaultAsync<bool>("SELECT sp_findings_update(@p_id, @p_organization_id, @p_title, @p_severity, @p_status, @p_description, @p_root_cause)", parameters);
        return updated ? finding : null;
    }

    public async Task<bool> DeleteFindingAsync(string id, int organizationId)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_findings_delete(@id, @organizationId)", new { id, organizationId });
    }
}
