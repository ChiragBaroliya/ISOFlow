using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class DocumentRepository : BaseRepository, IDocumentRepository
{
    public DocumentRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<List<Policy>> GetAllPoliciesAsync(int? organizationId)
    {
        return QueryMappedListAsync("SELECT * FROM sp_policies_get_all(@organizationId)", r => new Policy
        {
            Id = r.id.ToString(),
            OrganizationId = (int)r.organization_id,
            Code = (string)r.code,
            Title = (string)r.title,
            Version = (string)r.version,
            Owner = (string)r.owner,
            EffectiveDate = (DateTime)r.effective_date,
            NextReviewDate = (DateTime)r.next_review_date,
            Status = (string)r.status,
            FilePath = (string)r.file_path ?? string.Empty
        }, new { organizationId });
    }

    public Task<PagedResponse<Policy>> GetPagedPoliciesAsync(PagedRequestDto request, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", string.IsNullOrWhiteSpace(request.StatusFilter) ? null : request.StatusFilter.Trim());

        return QueryPagedAsync(
            "SELECT * FROM sp_policies_get_paged(@p_page_number, @p_page_size, @p_organization_id, @p_search_term, @p_status)",
            r => new Policy
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                Title = (string)r.title,
                Version = (string)r.version,
                Owner = (string)r.owner,
                EffectiveDate = (DateTime)r.effective_date,
                NextReviewDate = (DateTime)r.next_review_date,
                Status = (string)r.status,
                FilePath = (string)r.file_path ?? string.Empty
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<Policy?> GetPolicyByIdAsync(string id, int? organizationId)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_policies_get_by_id(@id, @organizationId)",
            r => new Policy
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                Title = (string)r.title,
                Version = (string)r.version,
                Owner = (string)r.owner,
                EffectiveDate = (DateTime)r.effective_date,
                NextReviewDate = (DateTime)r.next_review_date,
                Status = (string)r.status,
                FilePath = (string)r.file_path ?? string.Empty
            },
            new { id, organizationId });
    }

    public async Task<Policy> CreatePolicyAsync(Policy policy)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", policy.OrganizationId);
        parameters.Add("p_code", policy.Code);
        parameters.Add("p_title", policy.Title);
        parameters.Add("p_version", policy.Version);
        parameters.Add("p_owner", policy.Owner);
        parameters.Add("p_effective_date", policy.EffectiveDate);
        parameters.Add("p_next_review_date", policy.NextReviewDate);
        parameters.Add("p_status", policy.Status);
        parameters.Add("p_file_path", policy.FilePath);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_policies_create(@p_organization_id, @p_code, @p_title, @p_version, @p_owner, @p_effective_date, @p_next_review_date, @p_status, @p_file_path)", parameters);
        policy.Id = insertedId.ToString();
        return policy;
    }

    public async Task<Policy?> UpdatePolicyAsync(Policy policy, int organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", policy.Id);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_title", policy.Title);
        parameters.Add("p_version", policy.Version);
        parameters.Add("p_owner", policy.Owner);
        parameters.Add("p_effective_date", policy.EffectiveDate);
        parameters.Add("p_next_review_date", policy.NextReviewDate);
        parameters.Add("p_status", policy.Status);
        parameters.Add("p_file_path", policy.FilePath);

        var updated = await QuerySingleOrDefaultAsync<bool>("SELECT sp_policies_update(@p_id, @p_organization_id, @p_title, @p_version, @p_owner, @p_effective_date, @p_next_review_date, @p_status, @p_file_path)", parameters);
        return updated ? policy : null;
    }

    public async Task<bool> DeletePolicyAsync(string id, int organizationId)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_policies_delete(@id, @organizationId)", new { id, organizationId });
    }

    public Task<List<Process>> GetAllProcessesAsync(int? organizationId)
    {
        return QueryMappedListAsync(
            "SELECT id, organization_id, code, title, category, owner, description, version, status, policy_id FROM processes WHERE (@organizationId IS NULL OR organization_id = @organizationId) ORDER BY id ASC",
            pr => new Process
            {
                Id = pr.id.ToString(),
                OrganizationId = (int)pr.organization_id,
                Code = (string)pr.code,
                Title = (string)pr.title,
                Category = (string)pr.category,
                Owner = (string)pr.owner,
                Description = (string)pr.description ?? string.Empty,
                Version = (string)pr.version,
                Status = (string)pr.status,
                PolicyId = pr.policy_id != null ? pr.policy_id.ToString() : string.Empty
            },
            new { organizationId });
    }

    public Task<PagedResponse<Process>> GetPagedProcessesAsync(PagedRequestDto request, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_category", string.IsNullOrWhiteSpace(request.CategoryFilter) ? null : request.CategoryFilter.Trim());
        parameters.Add("p_status", string.IsNullOrWhiteSpace(request.StatusFilter) ? null : request.StatusFilter.Trim());

        return QueryPagedAsync(
            "SELECT * FROM sp_processes_get_paged(@p_page_number, @p_page_size, @p_organization_id, @p_search_term, @p_category, @p_status)",
            pr => new Process
            {
                Id = pr.id.ToString(),
                OrganizationId = (int)pr.organization_id,
                Code = (string)pr.code,
                Title = (string)pr.title,
                Category = (string)pr.category,
                Owner = (string)pr.owner,
                Description = (string)pr.description ?? string.Empty,
                Version = (string)pr.version,
                Status = (string)pr.status,
                PolicyId = pr.policy_id != null ? pr.policy_id.ToString() : string.Empty
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<Process?> GetProcessByIdAsync(string id, int? organizationId)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT id, organization_id, code, title, category, owner, description, version, status, policy_id FROM processes WHERE (id::VARCHAR = @id OR LOWER(code) = LOWER(@id)) AND (@organizationId IS NULL OR organization_id = @organizationId)",
            pr => new Process
            {
                Id = pr.id.ToString(),
                OrganizationId = (int)pr.organization_id,
                Code = (string)pr.code,
                Title = (string)pr.title,
                Category = (string)pr.category,
                Owner = (string)pr.owner,
                Description = (string)pr.description ?? string.Empty,
                Version = (string)pr.version,
                Status = (string)pr.status,
                PolicyId = pr.policy_id != null ? pr.policy_id.ToString() : string.Empty
            },
            new { id, organizationId });
    }

    public async Task<Process> CreateProcessAsync(Process process)
    {
        int.TryParse(process.PolicyId, out var polId);
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", process.OrganizationId);
        parameters.Add("p_code", process.Code);
        parameters.Add("p_title", process.Title);
        parameters.Add("p_category", process.Category);
        parameters.Add("p_owner", process.Owner);
        parameters.Add("p_description", process.Description);
        parameters.Add("p_version", process.Version);
        parameters.Add("p_status", string.IsNullOrWhiteSpace(process.Status) ? "Active" : process.Status);
        parameters.Add("p_policy_id", polId > 0 ? (int?)polId : null);

        var insertedId = await QuerySingleAsync<int>("INSERT INTO processes (organization_id, code, title, category, owner, description, version, status, policy_id) VALUES (@p_organization_id, @p_code, @p_title, @p_category, @p_owner, @p_description, @p_version, @p_status, @p_policy_id) RETURNING id", parameters);
        process.Id = insertedId.ToString();
        return process;
    }

    public async Task<Process?> UpdateProcessAsync(Process process, int organizationId)
    {
        int.TryParse(process.PolicyId, out var polId);
        var parameters = new DynamicParameters();
        parameters.Add("p_id", process.Id);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_title", process.Title);
        parameters.Add("p_category", process.Category);
        parameters.Add("p_owner", process.Owner);
        parameters.Add("p_description", process.Description);
        parameters.Add("p_version", process.Version);
        parameters.Add("p_status", process.Status);
        parameters.Add("p_policy_id", polId > 0 ? (int?)polId : null);

        var rows = await ExecuteAsync("UPDATE processes SET title = @p_title, category = @p_category, owner = @p_owner, description = @p_description, version = @p_version, status = @p_status, policy_id = @p_policy_id, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE (id::VARCHAR = @p_id OR LOWER(code) = LOWER(@p_id)) AND organization_id = @p_organization_id", parameters);
        return rows > 0 ? process : null;
    }

    public async Task<bool> ArchiveProcessAsync(string id, int organizationId)
    {
        int.TryParse(id, out var procId);
        var parameters = new DynamicParameters();
        parameters.Add("p_process_id", procId);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_success", dbType: System.Data.DbType.Boolean, direction: System.Data.ParameterDirection.InputOutput);

        await ExecuteAsync("CALL sp_process_archive(@p_process_id, @p_organization_id, @p_success)", parameters);
        return parameters.Get<bool>("p_success");
    }
}
