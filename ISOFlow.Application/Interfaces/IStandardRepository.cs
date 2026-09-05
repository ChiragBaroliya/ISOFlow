using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IStandardRepository
{
    Task<List<Standard>> GetAllStandardsAsync(int? organizationId);
    Task<PagedResponse<Standard>> GetPagedStandardsAsync(PagedRequestDto request, int? organizationId);
    Task<Standard?> GetStandardByIdAsync(string id, int? organizationId);
    Task<Standard> CreateStandardAsync(Standard standard);
    Task<Standard?> UpdateStandardAsync(Standard standard, int organizationId);
    Task<bool> DeleteStandardAsync(string id, int organizationId);

    Task<List<Requirement>> GetRequirementsByStandardIdAsync(string standardId, int? organizationId);
    Task<PagedResponse<Requirement>> GetPagedRequirementsByStandardIdAsync(string standardId, PagedRequestDto request, int? organizationId);
    Task<Requirement?> GetRequirementByIdAsync(string id, int? organizationId);
    Task<Requirement> CreateRequirementAsync(Requirement requirement);
    Task<Requirement?> UpdateRequirementAsync(Requirement requirement, int organizationId);
    Task<bool> DeleteRequirementAsync(string id, int organizationId);
}
