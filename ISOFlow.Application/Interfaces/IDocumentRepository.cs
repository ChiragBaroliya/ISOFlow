using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IDocumentRepository
{
    Task<List<Policy>> GetAllPoliciesAsync(int? organizationId);
    Task<PagedResponse<Policy>> GetPagedPoliciesAsync(PagedRequestDto request, int? organizationId);
    Task<Policy?> GetPolicyByIdAsync(string id, int? organizationId);
    Task<Policy> CreatePolicyAsync(Policy policy);
    Task<Policy?> UpdatePolicyAsync(Policy policy, int organizationId);
    Task<bool> DeletePolicyAsync(string id, int organizationId);

    Task<List<Process>> GetAllProcessesAsync(int? organizationId);
    Task<PagedResponse<Process>> GetPagedProcessesAsync(PagedRequestDto request, int? organizationId);
    Task<Process?> GetProcessByIdAsync(string id, int? organizationId);
    Task<Process> CreateProcessAsync(Process process);
    Task<Process?> UpdateProcessAsync(Process process, int organizationId);
    Task<bool> ArchiveProcessAsync(string id, int organizationId);
}
