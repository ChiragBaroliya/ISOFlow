using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Audits;

public interface IAuditsApiClient
{
    Task<List<Audit>> GetAllAuditsAsync();
    Task<Audit?> GetAuditByIdAsync(string id);
    Task<Audit?> CreateAuditAsync(Audit audit);
    Task<Audit?> UpdateAuditAsync(Audit audit);
    Task<bool> DeleteAuditAsync(string id);
}

public class AuditsApiClient : IAuditsApiClient
{
    private readonly IApiHttpClient _api;

    public AuditsApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<Audit>> GetAllAuditsAsync() =>
        await _api.GetAsync<List<Audit>>("api/audits/all") ?? new();

    public async Task<Audit?> GetAuditByIdAsync(string id) =>
        await _api.GetAsync<Audit>($"api/audits/{Uri.EscapeDataString(id)}");

    public async Task<Audit?> CreateAuditAsync(Audit audit) =>
        await _api.PostAsync<Audit>("api/audits", new
        {
            audit.Code, audit.Title, audit.StandardId, audit.LeadAuditor,
            audit.StartDate, audit.EndDate, audit.Status,
            audit.CompletionPercentage, audit.Scope, audit.CheckListControlIds
        });

    public async Task<Audit?> UpdateAuditAsync(Audit audit) =>
        await _api.PutAsync<Audit>($"api/audits/{Uri.EscapeDataString(audit.Id)}", new
        {
            audit.Code, audit.Title, audit.StandardId, audit.LeadAuditor,
            audit.StartDate, audit.EndDate, audit.Status,
            audit.CompletionPercentage, audit.Scope, audit.CheckListControlIds
        });

    public async Task<bool> DeleteAuditAsync(string id) =>
        await _api.DeleteAsync($"api/audits/{Uri.EscapeDataString(id)}");
}
