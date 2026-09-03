using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Capa;

public interface ICapaApiClient
{
    Task<List<CAPA>> GetAllCapasAsync();
    Task<CAPA?> GetCapaByIdAsync(string id);
    Task<CAPA?> CreateCapaAsync(CAPA capa);
    Task<CAPA?> UpdateCapaAsync(CAPA capa);
    Task<bool> DeleteCapaAsync(string id);
    Task<bool> AddActionItemAsync(string capaId, CapaActionItem item);
    Task<bool> ToggleActionItemAsync(string capaId, string actionItemId);
}

public class CapaApiClient : ICapaApiClient
{
    private readonly IApiHttpClient _api;

    public CapaApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<CAPA>> GetAllCapasAsync() =>
        await _api.GetAsync<List<CAPA>>("api/capa/all") ?? new();

    public async Task<CAPA?> GetCapaByIdAsync(string id) =>
        await _api.GetAsync<CAPA>($"api/capa/{Uri.EscapeDataString(id)}");

    public async Task<CAPA?> CreateCapaAsync(CAPA capa) =>
        await _api.PostAsync<CAPA>("api/capa", new
        {
            capa.Code, capa.FindingId, capa.Title, capa.RootCause,
            capa.CorrectiveAction, capa.Owner, capa.DueDate,
            capa.Status, capa.EffectivenessVerification
        });

    public async Task<CAPA?> UpdateCapaAsync(CAPA capa) =>
        await _api.PutAsync<CAPA>($"api/capa/{Uri.EscapeDataString(capa.Id)}", new
        {
            capa.Code, capa.FindingId, capa.Title, capa.RootCause,
            capa.CorrectiveAction, capa.Owner, capa.DueDate,
            capa.Status, capa.EffectivenessVerification
        });

    public async Task<bool> DeleteCapaAsync(string id) =>
        await _api.DeleteAsync($"api/capa/{Uri.EscapeDataString(id)}");

    public async Task<bool> AddActionItemAsync(string capaId, CapaActionItem item) =>
        await _api.PostBoolAsync($"api/capa/{Uri.EscapeDataString(capaId)}/action-items", new
        {
            item.Title, item.AssignedTo, item.DueDate, item.IsCompleted
        });

    public async Task<bool> ToggleActionItemAsync(string capaId, string actionItemId) =>
        await _api.PatchBoolAsync($"api/capa/{Uri.EscapeDataString(capaId)}/action-items/{Uri.EscapeDataString(actionItemId)}/toggle");
}
