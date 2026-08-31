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

    public Task<List<Audit>> GetAllAuditsAsync()
    {
        return QueryMappedListAsync("SELECT * FROM sp_audits_get_all()", r => new Audit
        {
            Id = r.id.ToString(),
            Code = (string)r.code,
            Title = (string)r.title,
            StandardId = r.standard_id != null ? r.standard_id.ToString() : string.Empty,
            LeadAuditor = (string)r.lead_auditor,
            StartDate = (DateTime)r.start_date,
            EndDate = (DateTime)r.end_date,
            Status = (AuditStatus)(int)r.status,
            CompletionPercentage = (int)r.completion_percentage,
            Scope = (string)r.scope ?? string.Empty
        });
    }

    public Task<PagedResponse<Audit>> GetPagedAuditsAsync(PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", int.TryParse(request.StatusFilter, out var st) ? st : (int?)null);

        return QueryPagedAsync(
            "SELECT * FROM sp_audits_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_status)",
            r => new Audit
            {
                Id = r.id.ToString(),
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

    public Task<Audit?> GetAuditByIdAsync(string id)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT id, code, title, standard_id, lead_auditor, start_date, end_date, status, completion_percentage, scope FROM audits WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)",
            r => new Audit
            {
                Id = r.id.ToString(),
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
            new { id });
    }

    public async Task<Audit> CreateAuditAsync(Audit audit)
    {
        int.TryParse(audit.StandardId, out var stdId);
        var parameters = new DynamicParameters();
        parameters.Add("p_code", audit.Code);
        parameters.Add("p_title", audit.Title);
        parameters.Add("p_standard_id", stdId > 0 ? (int?)stdId : null);
        parameters.Add("p_lead_auditor", audit.LeadAuditor);
        parameters.Add("p_start_date", audit.StartDate);
        parameters.Add("p_end_date", audit.EndDate);
        parameters.Add("p_status", (int)audit.Status);
        parameters.Add("p_completion_percentage", audit.CompletionPercentage);
        parameters.Add("p_scope", audit.Scope);

        var insertedId = await QuerySingleAsync<int>("INSERT INTO audits (code, title, standard_id, lead_auditor, start_date, end_date, status, completion_percentage, scope) VALUES (@p_code, @p_title, @p_standard_id, @p_lead_auditor, @p_start_date, @p_end_date, @p_status, @p_completion_percentage, @p_scope) RETURNING id", parameters);
        audit.Id = insertedId.ToString();
        return audit;
    }

    public async Task<Audit?> UpdateAuditAsync(Audit audit)
    {
        int.TryParse(audit.StandardId, out var stdId);
        var parameters = new DynamicParameters();
        parameters.Add("p_id", audit.Id);
        parameters.Add("p_title", audit.Title);
        parameters.Add("p_standard_id", stdId > 0 ? (int?)stdId : null);
        parameters.Add("p_lead_auditor", audit.LeadAuditor);
        parameters.Add("p_start_date", audit.StartDate);
        parameters.Add("p_end_date", audit.EndDate);
        parameters.Add("p_status", (int)audit.Status);
        parameters.Add("p_completion_percentage", audit.CompletionPercentage);
        parameters.Add("p_scope", audit.Scope);

        var rows = await ExecuteAsync("UPDATE audits SET title = @p_title, standard_id = @p_standard_id, lead_auditor = @p_lead_auditor, start_date = @p_start_date, end_date = @p_end_date, status = @p_status, completion_percentage = @p_completion_percentage, scope = @p_scope, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE id::VARCHAR = @p_id OR LOWER(code) = LOWER(@p_id)", parameters);
        return rows > 0 ? audit : null;
    }

    public async Task<bool> DeleteAuditAsync(string id)
    {
        var rows = await ExecuteAsync("DELETE FROM audits WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)", new { id });
        return rows > 0;
    }

    public Task<List<Finding>> GetAllFindingsAsync()
    {
        return QueryMappedListAsync(
            "SELECT id, code, title, audit_id, requirement_id, control_id, severity, status, description, root_cause, identified_date, auditor, capa_id FROM findings ORDER BY identified_date DESC",
            f => new Finding
            {
                Id = f.id.ToString(),
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
            });
    }

    public Task<PagedResponse<Finding>> GetPagedFindingsAsync(PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", int.TryParse(request.StatusFilter, out var st) ? st : (int?)null);

        return QueryPagedAsync(
            "SELECT * FROM sp_findings_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_status)",
            f => new Finding
            {
                Id = f.id.ToString(),
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

    public Task<Finding?> GetFindingByIdAsync(string id)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT id, code, title, audit_id, requirement_id, control_id, severity, status, description, root_cause, identified_date, auditor, capa_id FROM findings WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)",
            f => new Finding
            {
                Id = f.id.ToString(),
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
            new { id });
    }

    public async Task<Finding> CreateFindingAsync(Finding finding)
    {
        int.TryParse(finding.AuditId, out var auditId);
        int.TryParse(finding.RequirementId, out var reqId);
        int.TryParse(finding.ControlId, out var ctrlId);

        var parameters = new DynamicParameters();
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

        var insertedId = await QuerySingleAsync<int>("INSERT INTO findings (code, title, audit_id, requirement_id, control_id, severity, status, description, root_cause, auditor) VALUES (@p_code, @p_title, @p_audit_id, @p_requirement_id, @p_control_id, @p_severity, @p_status, @p_description, @p_root_cause, @p_auditor) RETURNING id", parameters);
        finding.Id = insertedId.ToString();
        return finding;
    }

    public async Task<Finding?> UpdateFindingAsync(Finding finding)
    {
        int.TryParse(finding.AuditId, out var auditId);
        int.TryParse(finding.RequirementId, out var reqId);
        int.TryParse(finding.ControlId, out var ctrlId);

        var parameters = new DynamicParameters();
        parameters.Add("p_id", finding.Id);
        parameters.Add("p_title", finding.Title);
        parameters.Add("p_audit_id", auditId > 0 ? (int?)auditId : null);
        parameters.Add("p_requirement_id", reqId > 0 ? (int?)reqId : null);
        parameters.Add("p_control_id", ctrlId > 0 ? (int?)ctrlId : null);
        parameters.Add("p_severity", (int)finding.Severity);
        parameters.Add("p_status", (int)finding.Status);
        parameters.Add("p_description", finding.Description);
        parameters.Add("p_root_cause", finding.RootCause);
        parameters.Add("p_auditor", finding.Auditor);

        var rows = await ExecuteAsync("UPDATE findings SET title = @p_title, audit_id = @p_audit_id, requirement_id = @p_requirement_id, control_id = @p_control_id, severity = @p_severity, status = @p_status, description = @p_description, root_cause = @p_root_cause, auditor = @p_auditor, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE id::VARCHAR = @p_id OR LOWER(code) = LOWER(@p_id)", parameters);
        return rows > 0 ? finding : null;
    }

    public async Task<bool> DeleteFindingAsync(string id)
    {
        var rows = await ExecuteAsync("DELETE FROM findings WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)", new { id });
        return rows > 0;
    }
}
