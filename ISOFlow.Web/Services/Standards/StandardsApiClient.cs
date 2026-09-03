using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Standards;

public interface IStandardsApiClient
{
    Task<List<Standard>> GetAllStandardsAsync();
    Task<Standard?> GetStandardByIdAsync(string id);
    Task<Standard?> CreateStandardAsync(Standard standard);
    Task<Standard?> UpdateStandardAsync(Standard standard);
    Task<bool> DeleteStandardAsync(string id);

    Task<List<Requirement>> GetRequirementsByStandardIdAsync(string standardId);
    Task<Requirement?> CreateRequirementAsync(string standardId, Requirement requirement);
    Task<Requirement?> UpdateRequirementAsync(string standardId, string reqId, Requirement requirement);
    Task<bool> DeleteRequirementAsync(string standardId, string reqId);
}

public class StandardsApiClient : IStandardsApiClient
{
    private readonly IApiHttpClient _api;

    public StandardsApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<Standard>> GetAllStandardsAsync() =>
        await _api.GetAsync<List<Standard>>("api/standards/all") ?? new();

    public async Task<Standard?> GetStandardByIdAsync(string id) =>
        await _api.GetAsync<Standard>($"api/standards/{Uri.EscapeDataString(id)}");

    public async Task<Standard?> CreateStandardAsync(Standard standard) =>
        await _api.PostAsync<Standard>("api/standards", new
        {
            standard.Code, standard.Name, standard.Revision,
            standard.Description, standard.CompliancePercentage, standard.Status
        });

    public async Task<Standard?> UpdateStandardAsync(Standard standard) =>
        await _api.PutAsync<Standard>($"api/standards/{Uri.EscapeDataString(standard.Id)}", new
        {
            standard.Code, standard.Name, standard.Revision,
            standard.Description, standard.CompliancePercentage, standard.Status
        });

    public async Task<bool> DeleteStandardAsync(string id) =>
        await _api.DeleteAsync($"api/standards/{Uri.EscapeDataString(id)}");

    public async Task<List<Requirement>> GetRequirementsByStandardIdAsync(string standardId)
    {
        var paged = await _api.GetAsync<PagedResponse<Requirement>>(
            $"api/standards/{Uri.EscapeDataString(standardId)}/requirements?pageSize=100");
        return paged?.Items ?? new();
    }

    public async Task<Requirement?> CreateRequirementAsync(string standardId, Requirement requirement) =>
        await _api.PostAsync<Requirement>($"api/standards/{Uri.EscapeDataString(standardId)}/requirements", new
        {
            StandardId = standardId, requirement.Clause, requirement.Title,
            requirement.Description, requirement.Category,
            requirement.CompliancePercentage, requirement.RelatedControlIds
        });

    public async Task<Requirement?> UpdateRequirementAsync(string standardId, string reqId, Requirement requirement) =>
        await _api.PutAsync<Requirement>($"api/standards/{Uri.EscapeDataString(standardId)}/requirements/{Uri.EscapeDataString(reqId)}", new
        {
            StandardId = standardId, requirement.Clause, requirement.Title,
            requirement.Description, requirement.Category,
            requirement.CompliancePercentage, requirement.RelatedControlIds
        });

    public async Task<bool> DeleteRequirementAsync(string standardId, string reqId) =>
        await _api.DeleteAsync($"api/standards/{Uri.EscapeDataString(standardId)}/requirements/{Uri.EscapeDataString(reqId)}");
}
