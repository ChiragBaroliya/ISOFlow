using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.MockData;

namespace ISOFlow.Infrastructure.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    public Task<List<Organization>> GetAllOrganizationsAsync() => Task.FromResult(MockStore.Organizations);

    public Task<Organization?> GetOrganizationByIdAsync(string id) =>
        Task.FromResult(MockStore.Organizations.FirstOrDefault(o => o.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || o.Code.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<Organization> CreateOrganizationAsync(Organization organization)
    {
        if (string.IsNullOrWhiteSpace(organization.Id))
        {
            organization.Id = "ORG-" + (MockStore.Organizations.Count + 1).ToString("D3");
        }
        if (string.IsNullOrWhiteSpace(organization.Code))
        {
            organization.Code = organization.Id;
        }
        organization.CreatedAt = DateTime.UtcNow;
        organization.Status = string.IsNullOrWhiteSpace(organization.Status) ? "Active" : organization.Status;
        MockStore.Organizations.Add(organization);
        return Task.FromResult(organization);
    }

    public Task<Organization?> UpdateOrganizationAsync(Organization organization)
    {
        var existing = MockStore.Organizations.FirstOrDefault(o => o.Id.Equals(organization.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Name = organization.Name;
            existing.Industry = organization.Industry;
            existing.Employees = organization.Employees;
            existing.Locations = organization.Locations ?? new List<string>();
            existing.PrimaryStandard = organization.PrimaryStandard;
            existing.Status = organization.Status;
            existing.CompliancePercentage = organization.CompliancePercentage;
            existing.ContactEmail = organization.ContactEmail;
        }
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteOrganizationAsync(string id)
    {
        var existing = MockStore.Organizations.FirstOrDefault(o => o.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            MockStore.Organizations.Remove(existing);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
