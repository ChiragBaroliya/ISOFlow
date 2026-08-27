using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.MockData;

namespace ISOFlow.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    public Task<List<Policy>> GetAllPoliciesAsync() => Task.FromResult(MockStore.Policies);

    public Task<Policy?> GetPolicyByIdAsync(string id) =>
        Task.FromResult(MockStore.Policies.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || p.Code.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<Policy> CreatePolicyAsync(Policy policy)
    {
        if (string.IsNullOrWhiteSpace(policy.Id))
        {
            policy.Id = MockStore.NextId("POL", MockStore.Policies.Count);
        }
        if (string.IsNullOrWhiteSpace(policy.Code))
        {
            policy.Code = policy.Id;
        }
        policy.Status = string.IsNullOrWhiteSpace(policy.Status) ? "Active" : policy.Status;
        policy.Version = string.IsNullOrWhiteSpace(policy.Version) ? "1.0" : policy.Version;
        MockStore.Policies.Add(policy);
        return Task.FromResult(policy);
    }

    public Task<Policy?> UpdatePolicyAsync(Policy policy)
    {
        var existing = MockStore.Policies.FirstOrDefault(p => p.Id.Equals(policy.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Title = policy.Title;
            existing.Version = policy.Version;
            existing.Owner = policy.Owner;
            existing.EffectiveDate = policy.EffectiveDate;
            existing.NextReviewDate = policy.NextReviewDate;
            existing.Status = policy.Status;
            existing.FilePath = policy.FilePath;
            existing.LinkedControlIds = policy.LinkedControlIds ?? new List<string>();
            existing.LinkedProcessIds = policy.LinkedProcessIds ?? new List<string>();
        }
        return Task.FromResult(existing);
    }

    public Task<bool> DeletePolicyAsync(string id)
    {
        var existing = MockStore.Policies.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            MockStore.Policies.Remove(existing);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<List<Process>> GetAllProcessesAsync() => Task.FromResult(MockStore.Processes);

    public Task<Process?> GetProcessByIdAsync(string id) =>
        Task.FromResult(MockStore.Processes.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || p.Code.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<Process> CreateProcessAsync(Process process)
    {
        if (string.IsNullOrWhiteSpace(process.Id))
        {
            process.Id = "PROC-" + (MockStore.Processes.Count + 1).ToString("D3");
        }
        if (string.IsNullOrWhiteSpace(process.Code))
        {
            process.Code = process.Id;
        }
        process.Status = string.IsNullOrWhiteSpace(process.Status) ? "Active" : process.Status;
        process.Version = string.IsNullOrWhiteSpace(process.Version) ? "1.0" : process.Version;
        MockStore.Processes.Add(process);
        return Task.FromResult(process);
    }

    public Task<Process?> UpdateProcessAsync(Process process)
    {
        var existing = MockStore.Processes.FirstOrDefault(p => p.Id.Equals(process.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Title = process.Title;
            existing.Category = process.Category;
            existing.Owner = process.Owner;
            existing.Description = process.Description;
            existing.Version = process.Version;
            existing.Status = process.Status;
            existing.PolicyId = process.PolicyId;
            existing.Steps = process.Steps ?? new List<string>();
            existing.ControlIds = process.ControlIds ?? new List<string>();
        }
        return Task.FromResult(existing);
    }

    public Task<bool> ArchiveProcessAsync(string id)
    {
        var existing = MockStore.Processes.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Status = "Archived";
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
