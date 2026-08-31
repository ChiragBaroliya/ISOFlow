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

    public Task<List<Evidence>> GetAllEvidenceAsync()
    {
        return QueryMappedListAsync("SELECT * FROM sp_evidence_get_all()", r => new Evidence
        {
            Id = r.id.ToString(),
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
        });
    }

    public Task<PagedResponse<Evidence>> GetPagedEvidenceAsync(PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", string.IsNullOrWhiteSpace(request.StatusFilter) ? null : request.StatusFilter.Trim());

        return QueryPagedAsync(
            "SELECT * FROM sp_evidence_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_status)",
            r => new Evidence
            {
                Id = r.id.ToString(),
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

    public Task<Evidence?> GetEvidenceByIdAsync(string id)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT id, code, name, type, control_id, requirement_id, task_id, audit_id, uploaded_by, upload_date, expiry_date, status, file_url FROM evidence WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)",
            r => new Evidence
            {
                Id = r.id.ToString(),
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
            new { id });
    }

    public async Task<Evidence> CreateEvidenceAsync(Evidence evidence)
    {
        int.TryParse(evidence.ControlId, out var ctrlId);
        int.TryParse(evidence.RequirementId, out var reqId);
        int.TryParse(evidence.TaskId, out var taskId);
        int.TryParse(evidence.AuditId, out var auditId);

        var parameters = new DynamicParameters();
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

        var insertedId = await QuerySingleAsync<int>("INSERT INTO evidence (code, name, type, control_id, requirement_id, task_id, audit_id, uploaded_by, expiry_date, status, file_url) VALUES (@p_code, @p_name, @p_type, @p_control_id, @p_requirement_id, @p_task_id, @p_audit_id, @p_uploaded_by, @p_expiry_date, @p_status, @p_file_url) RETURNING id", parameters);
        evidence.Id = insertedId.ToString();
        return evidence;
    }

    public async Task<Evidence?> UpdateEvidenceAsync(Evidence evidence)
    {
        int.TryParse(evidence.ControlId, out var ctrlId);
        int.TryParse(evidence.RequirementId, out var reqId);
        int.TryParse(evidence.TaskId, out var taskId);
        int.TryParse(evidence.AuditId, out var auditId);

        var parameters = new DynamicParameters();
        parameters.Add("p_id", evidence.Id);
        parameters.Add("p_name", evidence.Name);
        parameters.Add("p_type", (int)evidence.Type);
        parameters.Add("p_control_id", ctrlId > 0 ? (int?)ctrlId : null);
        parameters.Add("p_requirement_id", reqId > 0 ? (int?)reqId : null);
        parameters.Add("p_task_id", taskId > 0 ? (int?)taskId : null);
        parameters.Add("p_audit_id", auditId > 0 ? (int?)auditId : null);
        parameters.Add("p_expiry_date", evidence.ExpiryDate);
        parameters.Add("p_status", evidence.Status);
        parameters.Add("p_file_url", evidence.FileUrl);

        var rows = await ExecuteAsync("UPDATE evidence SET name = @p_name, type = @p_type, control_id = @p_control_id, requirement_id = @p_requirement_id, task_id = @p_task_id, audit_id = @p_audit_id, expiry_date = @p_expiry_date, status = @p_status, file_url = @p_file_url WHERE id::VARCHAR = @p_id OR LOWER(code) = LOWER(@p_id)", parameters);
        return rows > 0 ? evidence : null;
    }

    public async Task<bool> DeleteEvidenceAsync(string id)
    {
        var rows = await ExecuteAsync("DELETE FROM evidence WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)", new { id });
        return rows > 0;
    }
}
