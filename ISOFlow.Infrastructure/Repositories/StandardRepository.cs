using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.MockData;

namespace ISOFlow.Infrastructure.Repositories;

public class StandardRepository : IStandardRepository
{
    public Task<List<Standard>> GetAllStandardsAsync() => Task.FromResult(MockStore.Standards);

    public Task<Standard?> GetStandardByIdAsync(string id) =>
        Task.FromResult(MockStore.Standards.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || s.Code.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<List<Requirement>> GetRequirementsByStandardIdAsync(string standardId) =>
        Task.FromResult(MockStore.Requirements.Where(r => r.StandardId.Equals(standardId, StringComparison.OrdinalIgnoreCase)).ToList());

    public Task<Requirement?> GetRequirementByIdAsync(string id) =>
        Task.FromResult(MockStore.Requirements.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || r.Clause.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<Standard> CreateStandardAsync(Standard standard)
    {
        if (string.IsNullOrWhiteSpace(standard.Id))
        {
            standard.Id = "STD-" + (MockStore.Standards.Count + 1).ToString("D3");
        }
        if (string.IsNullOrWhiteSpace(standard.Code))
        {
            standard.Code = standard.Id;
        }
        standard.IsPreseeded = false;
        standard.Status = string.IsNullOrWhiteSpace(standard.Status) ? "Active" : standard.Status;
        MockStore.Standards.Add(standard);
        return Task.FromResult(standard);
    }

    public Task<Standard?> UpdateStandardAsync(Standard standard)
    {
        var existing = MockStore.Standards.FirstOrDefault(s => s.Id.Equals(standard.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Name = standard.Name;
            existing.Description = standard.Description;
            existing.Revision = standard.Revision;
            existing.CompliancePercentage = standard.CompliancePercentage;
            existing.RequirementCount = standard.RequirementCount;
            if (!existing.IsPreseeded)
            {
                existing.Code = standard.Code;
                existing.Status = standard.Status;
            }
        }
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteStandardAsync(string id)
    {
        var existing = MockStore.Standards.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null && !existing.IsPreseeded)
        {
            MockStore.Standards.Remove(existing);
            MockStore.Requirements.RemoveAll(r => r.StandardId.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<Requirement> CreateRequirementAsync(Requirement requirement)
    {
        if (string.IsNullOrWhiteSpace(requirement.Id))
        {
            requirement.Id = MockStore.NextId("REQ", MockStore.Requirements.Count);
        }
        if (string.IsNullOrWhiteSpace(requirement.Clause))
        {
            requirement.Clause = requirement.Id;
        }
        MockStore.Requirements.Add(requirement);

        // Update requirement count on parent standard
        var std = MockStore.Standards.FirstOrDefault(s => s.Id.Equals(requirement.StandardId, StringComparison.OrdinalIgnoreCase));
        if (std != null)
        {
            std.RequirementCount = MockStore.Requirements.Count(r => r.StandardId.Equals(requirement.StandardId, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult(requirement);
    }

    public Task<Requirement?> UpdateRequirementAsync(Requirement requirement)
    {
        var existing = MockStore.Requirements.FirstOrDefault(r => r.Id.Equals(requirement.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Clause = requirement.Clause;
            existing.Title = requirement.Title;
            existing.Category = requirement.Category;
            existing.Description = requirement.Description;
            existing.CompliancePercentage = requirement.CompliancePercentage;
            existing.RelatedControlIds = requirement.RelatedControlIds ?? new List<string>();
        }
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteRequirementAsync(string id)
    {
        var existing = MockStore.Requirements.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            var stdId = existing.StandardId;
            MockStore.Requirements.Remove(existing);

            var std = MockStore.Standards.FirstOrDefault(s => s.Id.Equals(stdId, StringComparison.OrdinalIgnoreCase));
            if (std != null)
            {
                std.RequirementCount = MockStore.Requirements.Count(r => r.StandardId.Equals(stdId, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
