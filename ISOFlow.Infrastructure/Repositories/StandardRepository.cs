using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class StandardRepository : BaseRepository, IStandardRepository
{
    public StandardRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<List<Standard>> GetAllStandardsAsync(int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedListAsync("SELECT * FROM sp_standards_get_all(@p_organization_id)", r => new Standard
        {
            Id = r.id.ToString(),
            OrganizationId = (int)r.organization_id,
            Code = (string)r.code,
            Name = (string)r.name,
            Revision = (string)r.revision,
            Description = (string)r.description ?? string.Empty,
            RequirementCount = (int)r.requirement_count,
            CompliancePercentage = (double)r.compliance_percentage,
            IsPreseeded = (bool)r.is_preseeded,
            Status = (string)r.status
        }, parameters);
    }

    public Task<PagedResponse<Standard>> GetPagedStandardsAsync(PagedRequestDto request, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", string.IsNullOrWhiteSpace(request.StatusFilter) ? null : request.StatusFilter.Trim());

        return QueryPagedAsync(
            "SELECT * FROM sp_standards_get_paged(@p_page_number, @p_page_size, @p_organization_id, @p_search_term, @p_status)",
            r => new Standard
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                Name = (string)r.name,
                Revision = (string)r.revision,
                Description = (string)r.description ?? string.Empty,
                RequirementCount = (int)r.requirement_count,
                CompliancePercentage = (double)r.compliance_percentage,
                IsPreseeded = (bool)r.is_preseeded,
                Status = (string)r.status
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<Standard?> GetStandardByIdAsync(string id, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_standards_get_by_id(@id, @p_organization_id)",
            r => new Standard
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                Name = (string)r.name,
                Revision = (string)r.revision,
                Description = (string)r.description ?? string.Empty,
                RequirementCount = (int)r.requirement_count,
                CompliancePercentage = (double)r.compliance_percentage,
                IsPreseeded = (bool)r.is_preseeded,
                Status = (string)r.status
            },
            parameters);
    }

    public Task<List<Requirement>> GetRequirementsByStandardIdAsync(string standardId, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("standardId", standardId);
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedListAsync(
            "SELECT * FROM sp_requirements_get_by_standard_id(@standardId, @p_organization_id)",
            r => new Requirement
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                StandardId = r.standard_id.ToString(),
                Clause = (string)r.clause,
                Title = (string)r.title,
                Description = (string)r.description ?? string.Empty,
                Category = (string)r.category,
                CompliancePercentage = (double)r.compliance_percentage
            },
            parameters);
    }

    public Task<PagedResponse<Requirement>> GetPagedRequirementsByStandardIdAsync(string standardId, PagedRequestDto request, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_standard_id", standardId);
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());

        return QueryPagedAsync(
            "SELECT * FROM sp_requirements_get_paged_by_standard_id(@p_standard_id, @p_page_number, @p_page_size, @p_organization_id, @p_search_term)",
            r => new Requirement
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                StandardId = r.standard_id.ToString(),
                Clause = (string)r.clause,
                Title = (string)r.title,
                Description = (string)r.description ?? string.Empty,
                Category = (string)r.category,
                CompliancePercentage = (double)r.compliance_percentage
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<Requirement?> GetRequirementByIdAsync(string id, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_requirements_get_by_id(@id, @p_organization_id)",
            r => new Requirement
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                StandardId = r.standard_id.ToString(),
                Clause = (string)r.clause,
                Title = (string)r.title,
                Description = (string)r.description ?? string.Empty,
                Category = (string)r.category,
                CompliancePercentage = (double)r.compliance_percentage
            },
            parameters);
    }

    public async Task<Standard> CreateStandardAsync(Standard standard)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", standard.OrganizationId);
        parameters.Add("p_code", standard.Code);
        parameters.Add("p_name", standard.Name);
        parameters.Add("p_revision", standard.Revision);
        parameters.Add("p_description", standard.Description);
        parameters.Add("p_requirement_count", standard.RequirementCount);
        parameters.Add("p_compliance_percentage", standard.CompliancePercentage);
        parameters.Add("p_is_preseeded", standard.IsPreseeded);
        parameters.Add("p_status", string.IsNullOrWhiteSpace(standard.Status) ? "Active" : standard.Status);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_standards_create(@p_organization_id, @p_code, @p_name, @p_revision, @p_description, @p_requirement_count, @p_compliance_percentage, @p_is_preseeded, @p_status)", parameters);
        standard.Id = insertedId.ToString();
        return standard;
    }

    public async Task<Standard?> UpdateStandardAsync(Standard standard, int organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", standard.Id);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_name", standard.Name);
        parameters.Add("p_revision", standard.Revision);
        parameters.Add("p_description", standard.Description);
        parameters.Add("p_requirement_count", standard.RequirementCount);
        parameters.Add("p_compliance_percentage", standard.CompliancePercentage);

        var updated = await QuerySingleOrDefaultAsync<bool>("SELECT sp_standards_update(@p_id, @p_organization_id, @p_name, @p_revision, @p_description, @p_requirement_count, @p_compliance_percentage)", parameters);
        return updated ? standard : null;
    }

    public async Task<bool> DeleteStandardAsync(string id, int organizationId)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_standards_delete(@id, @organizationId)", new { id, organizationId });
    }

    public async Task<Requirement> CreateRequirementAsync(Requirement requirement)
    {
        int.TryParse(requirement.StandardId, out var stdId);
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", requirement.OrganizationId);
        parameters.Add("p_standard_id", stdId);
        parameters.Add("p_clause", requirement.Clause);
        parameters.Add("p_title", requirement.Title);
        parameters.Add("p_description", requirement.Description);
        parameters.Add("p_category", requirement.Category);
        parameters.Add("p_compliance_percentage", requirement.CompliancePercentage);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_requirements_create(@p_organization_id, @p_standard_id, @p_clause, @p_title, @p_description, @p_category, @p_compliance_percentage)", parameters);
        requirement.Id = insertedId.ToString();
        return requirement;
    }

    public async Task<Requirement?> UpdateRequirementAsync(Requirement requirement, int organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", requirement.Id);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_clause", requirement.Clause);
        parameters.Add("p_title", requirement.Title);
        parameters.Add("p_description", requirement.Description);
        parameters.Add("p_category", requirement.Category);
        parameters.Add("p_compliance_percentage", requirement.CompliancePercentage);

        var updated = await QuerySingleOrDefaultAsync<bool>("SELECT sp_requirements_update(@p_id, @p_organization_id, @p_clause, @p_title, @p_description, @p_category, @p_compliance_percentage)", parameters);
        return updated ? requirement : null;
    }

    public async Task<bool> DeleteRequirementAsync(string id, int organizationId)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_requirements_delete(@id, @organizationId)", new { id, organizationId });
    }
}
