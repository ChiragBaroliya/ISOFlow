using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Organizations;

public interface IOrganizationsApiClient
{
    Task<List<Organization>> GetAllOrganizationsAsync();
    Task<Organization?> GetOrganizationByIdAsync(string id);
    Task<Organization?> CreateOrganizationAsync(Organization org);
    Task<Organization?> UpdateOrganizationAsync(Organization org);
    Task<bool> DeleteOrganizationAsync(string id);
}

public class OrganizationsApiClient : IOrganizationsApiClient
{
    private readonly IApiHttpClient _api;

    public OrganizationsApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<Organization>> GetAllOrganizationsAsync() =>
        await _api.GetAsync<List<Organization>>("api/organizations/all") ?? new();

    public async Task<Organization?> GetOrganizationByIdAsync(string id) =>
        await _api.GetAsync<Organization>($"api/organizations/{Uri.EscapeDataString(id)}");

    public async Task<Organization?> CreateOrganizationAsync(Organization org) =>
        await _api.PostAsync<Organization>("api/organizations", new
        {
            org.Code, org.Name, org.Industry, org.Employees, org.Locations,
            org.PrimaryStandard, org.Status, org.CompliancePercentage, org.ContactEmail
        });

    public async Task<Organization?> UpdateOrganizationAsync(Organization org) =>
        await _api.PutAsync<Organization>($"api/organizations/{Uri.EscapeDataString(org.Id)}", new
        {
            org.Code, org.Name, org.Industry, org.Employees, org.Locations,
            org.PrimaryStandard, org.Status, org.CompliancePercentage, org.ContactEmail
        });

    public async Task<bool> DeleteOrganizationAsync(string id) =>
        await _api.DeleteAsync($"api/organizations/{Uri.EscapeDataString(id)}");
}
