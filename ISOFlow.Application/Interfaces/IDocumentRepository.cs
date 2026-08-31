using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IDocumentRepository
{
    Task<List<Policy>> GetAllPoliciesAsync();
    Task<PagedResponse<Policy>> GetPagedPoliciesAsync(PagedRequestDto request);
    Task<Policy?> GetPolicyByIdAsync(string id);
    Task<Policy> CreatePolicyAsync(Policy policy);
    Task<Policy?> UpdatePolicyAsync(Policy policy);
    Task<bool> DeletePolicyAsync(string id);

    Task<List<Process>> GetAllProcessesAsync();
    Task<PagedResponse<Process>> GetPagedProcessesAsync(PagedRequestDto request);
    Task<Process?> GetProcessByIdAsync(string id);
    Task<Process> CreateProcessAsync(Process process);
    Task<Process?> UpdateProcessAsync(Process process);
    Task<bool> ArchiveProcessAsync(string id);
}
