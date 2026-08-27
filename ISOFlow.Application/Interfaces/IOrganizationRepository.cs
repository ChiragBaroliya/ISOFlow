using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IOrganizationRepository
{
    Task<List<Organization>> GetAllOrganizationsAsync();
    Task<Organization?> GetOrganizationByIdAsync(string id);
    Task<Organization> CreateOrganizationAsync(Organization organization);
    Task<Organization?> UpdateOrganizationAsync(Organization organization);
    Task<bool> DeleteOrganizationAsync(string id);
}
