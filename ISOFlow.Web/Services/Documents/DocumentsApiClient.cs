using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Documents;

public interface IDocumentsApiClient
{
    Task<List<Policy>> GetAllPoliciesAsync();
    Task<Policy?> CreatePolicyAsync(Policy policy);
    Task<Policy?> UpdatePolicyAsync(Policy policy);
    Task<bool> DeletePolicyAsync(string id);

    Task<List<Process>> GetAllProcessesAsync();
    Task<Process?> GetProcessByIdAsync(string id);
    Task<Process?> CreateProcessAsync(Process process);
    Task<Process?> UpdateProcessAsync(Process process);
    Task<bool> ArchiveProcessAsync(string id);
}

public class DocumentsApiClient : IDocumentsApiClient
{
    private readonly IApiHttpClient _api;

    public DocumentsApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<Policy>> GetAllPoliciesAsync()
    {
        var paged = await _api.GetAsync<PagedResponse<Policy>>("api/documents/policies?pageSize=100");
        return paged?.Items ?? new();
    }

    public async Task<Policy?> CreatePolicyAsync(Policy policy) =>
        await _api.PostAsync<Policy>("api/documents/policies", new
        {
            policy.Code, policy.Title, policy.Version, policy.Owner,
            policy.EffectiveDate, policy.NextReviewDate, policy.Status,
            policy.FilePath, policy.LinkedControlIds, policy.LinkedProcessIds
        });

    public async Task<Policy?> UpdatePolicyAsync(Policy policy) =>
        await _api.PutAsync<Policy>($"api/documents/policies/{Uri.EscapeDataString(policy.Id)}", new
        {
            policy.Code, policy.Title, policy.Version, policy.Owner,
            policy.EffectiveDate, policy.NextReviewDate, policy.Status,
            policy.FilePath, policy.LinkedControlIds, policy.LinkedProcessIds
        });

    public async Task<bool> DeletePolicyAsync(string id) =>
        await _api.DeleteAsync($"api/documents/policies/{Uri.EscapeDataString(id)}");

    public async Task<List<Process>> GetAllProcessesAsync()
    {
        var paged = await _api.GetAsync<PagedResponse<Process>>("api/documents/processes?pageSize=100");
        return paged?.Items ?? new();
    }

    public async Task<Process?> GetProcessByIdAsync(string id) =>
        await _api.GetAsync<Process>($"api/documents/processes/{Uri.EscapeDataString(id)}");

    public async Task<Process?> CreateProcessAsync(Process process) =>
        await _api.PostAsync<Process>("api/documents/processes", new
        {
            process.Code, process.Title, process.Category, process.Owner,
            process.Description, process.Version, process.Status,
            process.Steps, process.PolicyId, process.ControlIds
        });

    public async Task<Process?> UpdateProcessAsync(Process process) =>
        await _api.PutAsync<Process>($"api/documents/processes/{Uri.EscapeDataString(process.Id)}", new
        {
            process.Code, process.Title, process.Category, process.Owner,
            process.Description, process.Version, process.Status,
            process.Steps, process.PolicyId, process.ControlIds
        });

    public async Task<bool> ArchiveProcessAsync(string id) =>
        await _api.PostBoolAsync($"api/documents/processes/{Uri.EscapeDataString(id)}/archive");
}
