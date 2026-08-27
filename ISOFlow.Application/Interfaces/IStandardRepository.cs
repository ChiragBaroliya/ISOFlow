using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IStandardRepository
{
    Task<List<Standard>> GetAllStandardsAsync();
    Task<Standard?> GetStandardByIdAsync(string id);
    Task<Standard> CreateStandardAsync(Standard standard);
    Task<Standard?> UpdateStandardAsync(Standard standard);
    Task<bool> DeleteStandardAsync(string id);

    Task<List<Requirement>> GetRequirementsByStandardIdAsync(string standardId);
    Task<Requirement?> GetRequirementByIdAsync(string id);
    Task<Requirement> CreateRequirementAsync(Requirement requirement);
    Task<Requirement?> UpdateRequirementAsync(Requirement requirement);
    Task<bool> DeleteRequirementAsync(string id);
}
