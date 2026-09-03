using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Findings;

public interface IFindingsApiClient
{
    Task<List<Finding>> GetAllFindingsAsync();
    Task<Finding?> GetFindingByIdAsync(string id);
    Task<Finding?> CreateFindingAsync(Finding finding);
    Task<Finding?> UpdateFindingAsync(Finding finding);
    Task<bool> DeleteFindingAsync(string id);
}

public class FindingsApiClient : IFindingsApiClient
{
    private readonly IApiHttpClient _api;

    public FindingsApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<Finding>> GetAllFindingsAsync() =>
        await _api.GetAsync<List<Finding>>("api/findings/all") ?? new();

    public async Task<Finding?> GetFindingByIdAsync(string id) =>
        await _api.GetAsync<Finding>($"api/findings/{Uri.EscapeDataString(id)}");

    public async Task<Finding?> CreateFindingAsync(Finding finding) =>
        await _api.PostAsync<Finding>("api/findings", new
        {
            finding.Code, finding.Title, finding.AuditId, finding.RequirementId,
            finding.ControlId, finding.Severity, finding.Status, finding.Description,
            finding.RootCause, finding.IdentifiedDate, finding.Auditor, finding.CapaId
        });

    public async Task<Finding?> UpdateFindingAsync(Finding finding) =>
        await _api.PutAsync<Finding>($"api/findings/{Uri.EscapeDataString(finding.Id)}", new
        {
            finding.Code, finding.Title, finding.AuditId, finding.RequirementId,
            finding.ControlId, finding.Severity, finding.Status, finding.Description,
            finding.RootCause, finding.IdentifiedDate, finding.Auditor, finding.CapaId
        });

    public async Task<bool> DeleteFindingAsync(string id) =>
        await _api.DeleteAsync($"api/findings/{Uri.EscapeDataString(id)}");
}
