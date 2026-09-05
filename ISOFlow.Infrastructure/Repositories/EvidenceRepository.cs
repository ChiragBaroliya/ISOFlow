using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class EvidenceRepository : BaseRepository, IEvidenceRepository
{
    public EvidenceRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<List<Evidence>> GetAllEvidenceAsync(int? organizationId)
    {
        return QueryMappedListAsync(
            "SELECT * FROM sp_evidence_get_all(@p_organization_id)",
            r => new Evidence
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                Name = (string)r.name,
                Type = (EvidenceType)(int)r.type,
                ControlId = r.control_id != null ? r.control_id.ToString() : string.Empty,
                RequirementId = r.requirement_id != null ? r.requirement_id.ToString() : string.Empty,
                TaskId = r.task_id != null ? r.task_id.ToString() : string.Empty,
                AuditId = r.audit_id != null ? r.audit_id.ToString() : string.Empty,
                UploadedBy = (string)r.uploaded_by,
                UploadDate = (DateTime)r.upload_date,
                ExpiryDate = (DateTime)r.expiry_date,
                Status = (string)r.status,
                FileUrl = (string)r.file_url ?? string.Empty
            },
            new { p_organization_id = organizationId });
    }

    public Task<PagedResponse<Evidence>> GetPagedEvidenceAsync(PagedRequestDto request, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", string.IsNullOrWhiteSpace(request.StatusFilter) ? null : request.StatusFilter.Trim());

        return QueryPagedAsync(
            "SELECT * FROM sp_evidence_get_paged(@p_page_number, @p_page_size, @p_organization_id, @p_search_term, @p_status)",
            r => new Evidence
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                Name = (string)r.name,
                Type = (EvidenceType)(int)r.type,
                ControlId = r.control_id != null ? r.control_id.ToString() : string.Empty,
                RequirementId = r.requirement_id != null ? r.requirement_id.ToString() : string.Empty,
                TaskId = r.task_id != null ? r.task_id.ToString() : string.Empty,
                AuditId = r.audit_id != null ? r.audit_id.ToString() : string.Empty,
                UploadedBy = (string)r.uploaded_by,
                UploadDate = (DateTime)r.upload_date,
                ExpiryDate = (DateTime)r.expiry_date,
                Status = (string)r.status,
                FileUrl = (string)r.file_url ?? string.Empty
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<Evidence?> GetEvidenceByIdAsync(string id, int? organizationId)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_evidence_get_by_id(@id, @p_organization_id)",
            r => new Evidence
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                Name = (string)r.name,
                Type = (EvidenceType)(int)r.type,
                ControlId = r.control_id != null ? r.control_id.ToString() : string.Empty,
                RequirementId = r.requirement_id != null ? r.requirement_id.ToString() : string.Empty,
                TaskId = r.task_id != null ? r.task_id.ToString() : string.Empty,
                AuditId = r.audit_id != null ? r.audit_id.ToString() : string.Empty,
                UploadedBy = (string)r.uploaded_by,
                UploadDate = (DateTime)r.upload_date,
                ExpiryDate = (DateTime)r.expiry_date,
                Status = (string)r.status,
                FileUrl = (string)r.file_url ?? string.Empty
            },
            new { id, p_organization_id = organizationId });
    }

    public async Task<Evidence> CreateEvidenceAsync(Evidence evidence)
    {
        int.TryParse(evidence.ControlId, out var ctrlId);
        int.TryParse(evidence.RequirementId, out var reqId);
        int.TryParse(evidence.TaskId, out var taskId);
        int.TryParse(evidence.AuditId, out var auditId);

        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", evidence.OrganizationId);
        parameters.Add("p_code", evidence.Code);
        parameters.Add("p_name", evidence.Name);
        parameters.Add("p_type", (int)evidence.Type);
        parameters.Add("p_control_id", ctrlId > 0 ? (int?)ctrlId : null);
        parameters.Add("p_requirement_id", reqId > 0 ? (int?)reqId : null);
        parameters.Add("p_task_id", taskId > 0 ? (int?)taskId : null);
        parameters.Add("p_audit_id", auditId > 0 ? (int?)auditId : null);
        parameters.Add("p_uploaded_by", evidence.UploadedBy);
        parameters.Add("p_expiry_date", evidence.ExpiryDate);
        parameters.Add("p_status", string.IsNullOrWhiteSpace(evidence.Status) ? "Verified" : evidence.Status);
        parameters.Add("p_file_url", evidence.FileUrl);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_evidence_create(@p_organization_id::INT, @p_code::VARCHAR, @p_name::VARCHAR, @p_type::INT, @p_control_id::INT, @p_requirement_id::INT, @p_task_id::INT, @p_audit_id::INT, @p_uploaded_by::VARCHAR, @p_expiry_date::TIMESTAMP, @p_status::VARCHAR, @p_file_url::VARCHAR)", parameters);
        evidence.Id = insertedId.ToString();
        return evidence;
    }

    public async Task<Evidence?> UpdateEvidenceAsync(Evidence evidence, int organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", evidence.Id);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_name", evidence.Name);
        parameters.Add("p_expiry_date", evidence.ExpiryDate);
        parameters.Add("p_status", evidence.Status);
        parameters.Add("p_file_url", evidence.FileUrl);

        var updated = await QuerySingleOrDefaultAsync<bool>("SELECT sp_evidence_update(@p_id::VARCHAR, @p_organization_id::INT, @p_name::VARCHAR, @p_expiry_date::TIMESTAMP, @p_status::VARCHAR, @p_file_url::VARCHAR)", parameters);
        return updated ? evidence : null;
    }

    public async Task<bool> DeleteEvidenceAsync(string id, int organizationId)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_evidence_delete(@id, @p_organization_id)", new { id, p_organization_id = organizationId });
    }
}
