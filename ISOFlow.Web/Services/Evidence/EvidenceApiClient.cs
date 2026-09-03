using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Evidence;

public interface IEvidenceApiClient
{
    Task<List<Domain.Entities.Evidence>> GetAllEvidenceAsync();
    Task<Domain.Entities.Evidence?> CreateEvidenceAsync(Domain.Entities.Evidence evidence);
    Task<Domain.Entities.Evidence?> UpdateEvidenceAsync(Domain.Entities.Evidence evidence);
    Task<bool> DeleteEvidenceAsync(string id);
}

public class EvidenceApiClient : IEvidenceApiClient
{
    private readonly IApiHttpClient _api;

    public EvidenceApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<Domain.Entities.Evidence>> GetAllEvidenceAsync() =>
        await _api.GetAsync<List<Domain.Entities.Evidence>>("api/evidence/all") ?? new();

    public async Task<Domain.Entities.Evidence?> CreateEvidenceAsync(Domain.Entities.Evidence evidence) =>
        await _api.PostAsync<Domain.Entities.Evidence>("api/evidence", new
        {
            evidence.Code, evidence.Name, evidence.Type, evidence.ControlId,
            evidence.RequirementId, evidence.TaskId, evidence.AuditId,
            evidence.UploadedBy, evidence.UploadDate, evidence.ExpiryDate,
            evidence.Status, evidence.FileUrl
        });

    public async Task<Domain.Entities.Evidence?> UpdateEvidenceAsync(Domain.Entities.Evidence evidence) =>
        await _api.PutAsync<Domain.Entities.Evidence>($"api/evidence/{Uri.EscapeDataString(evidence.Id)}", new
        {
            evidence.Code, evidence.Name, evidence.Type, evidence.ControlId,
            evidence.RequirementId, evidence.TaskId, evidence.AuditId,
            evidence.UploadedBy, evidence.UploadDate, evidence.ExpiryDate,
            evidence.Status, evidence.FileUrl
        });

    public async Task<bool> DeleteEvidenceAsync(string id) =>
        await _api.DeleteAsync($"api/evidence/{Uri.EscapeDataString(id)}");
}
