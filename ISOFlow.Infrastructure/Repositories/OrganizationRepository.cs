using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class OrganizationRepository : BaseRepository, IOrganizationRepository
{
    public OrganizationRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<List<Organization>> GetAllOrganizationsAsync()
    {
        return QueryMappedListAsync("SELECT * FROM sp_organizations_get_all()", r => new Organization
        {
            Id = (int)r.id,
            Code = (string)r.code,
            Name = (string)r.name,
            Industry = (string)r.industry,
            Employees = (int)r.employees,
            PrimaryStandard = (string)r.primary_standard,
            Status = (string)r.status,
            CompliancePercentage = (double)r.compliance_percentage,
            ContactEmail = (string)r.contact_email,
            CreatedAt = (DateTime)r.created_at
        });
    }

    public Task<PagedResponse<Organization>> GetPagedOrganizationsAsync(PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", string.IsNullOrWhiteSpace(request.StatusFilter) ? null : request.StatusFilter.Trim());

        return QueryPagedAsync(
            "SELECT * FROM sp_organizations_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_status)",
            r => new Organization
            {
                Id = (int)r.id,
                Code = (string)r.code,
                Name = (string)r.name,
                Industry = (string)r.industry,
                Employees = (int)r.employees,
                PrimaryStandard = (string)r.primary_standard,
                Status = (string)r.status,
                CompliancePercentage = (double)r.compliance_percentage,
                ContactEmail = (string)r.contact_email,
                CreatedAt = (DateTime)r.created_at
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<Organization?> GetOrganizationByIdAsync(string id)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT id, code, name, industry, employees, primary_standard, status, compliance_percentage, contact_email, created_at FROM organizations WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)",
            r => new Organization
            {
                Id = (int)r.id,
                Code = (string)r.code,
                Name = (string)r.name,
                Industry = (string)r.industry,
                Employees = (int)r.employees,
                PrimaryStandard = (string)r.primary_standard,
                Status = (string)r.status,
                CompliancePercentage = (double)r.compliance_percentage,
                ContactEmail = (string)r.contact_email,
                CreatedAt = (DateTime)r.created_at
            },
            new { id });
    }

    public async Task<Organization> CreateOrganizationAsync(Organization organization)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_code", organization.Code);
        parameters.Add("p_name", organization.Name);
        parameters.Add("p_industry", organization.Industry);
        parameters.Add("p_employees", organization.Employees);
        parameters.Add("p_primary_standard", organization.PrimaryStandard);
        parameters.Add("p_status", string.IsNullOrWhiteSpace(organization.Status) ? "Active" : organization.Status);
        parameters.Add("p_compliance_percentage", organization.CompliancePercentage);
        parameters.Add("p_contact_email", organization.ContactEmail);

        var insertedId = await QuerySingleAsync<int>(
            "INSERT INTO organizations (code, name, industry, employees, primary_standard, status, compliance_percentage, contact_email) VALUES (@p_code, @p_name, @p_industry, @p_employees, @p_primary_standard, @p_status, @p_compliance_percentage, @p_contact_email) RETURNING id",
            parameters);

        organization.Id = insertedId;
        return organization;
    }

    public async Task<Organization?> UpdateOrganizationAsync(Organization organization)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", organization.Id.ToString());
        parameters.Add("p_name", organization.Name);
        parameters.Add("p_industry", organization.Industry);
        parameters.Add("p_employees", organization.Employees);
        parameters.Add("p_primary_standard", organization.PrimaryStandard);
        parameters.Add("p_status", organization.Status);
        parameters.Add("p_compliance_percentage", organization.CompliancePercentage);
        parameters.Add("p_contact_email", organization.ContactEmail);

        var rows = await ExecuteAsync(
            "UPDATE organizations SET name = @p_name, industry = @p_industry, employees = @p_employees, primary_standard = @p_primary_standard, status = @p_status, compliance_percentage = @p_compliance_percentage, contact_email = @p_contact_email, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE id::VARCHAR = @p_id OR LOWER(code) = LOWER(@p_id)",
            parameters);

        return rows > 0 ? organization : null;
    }

    public async Task<bool> DeleteOrganizationAsync(string id)
    {
        var rows = await ExecuteAsync("DELETE FROM organizations WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)", new { id });
        return rows > 0;
    }
}
