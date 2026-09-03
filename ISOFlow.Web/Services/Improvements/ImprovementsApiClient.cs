using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Improvements;

public interface IImprovementsApiClient
{
    Task<List<Improvement>> GetAllImprovementsAsync();
    Task<Improvement?> CreateImprovementAsync(Improvement improvement);
    Task<Improvement?> UpdateImprovementAsync(Improvement improvement);
    Task<bool> DeleteImprovementAsync(string id);
}

public class ImprovementsApiClient : IImprovementsApiClient
{
    private readonly IApiHttpClient _api;

    public ImprovementsApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<Improvement>> GetAllImprovementsAsync() =>
        await _api.GetAsync<List<Improvement>>("api/improvements/all") ?? new();

    public async Task<Improvement?> CreateImprovementAsync(Improvement improvement) =>
        await _api.PostAsync<Improvement>("api/improvements", new
        {
            improvement.Code, improvement.Title, improvement.CurrentState,
            improvement.FutureState, improvement.Source, improvement.ExpectedBenefit,
            improvement.Owner, improvement.Status, improvement.RelatedReviewId,
            improvement.RelatedFindingId
        });

    public async Task<Improvement?> UpdateImprovementAsync(Improvement improvement) =>
        await _api.PutAsync<Improvement>($"api/improvements/{Uri.EscapeDataString(improvement.Id)}", new
        {
            improvement.Code, improvement.Title, improvement.CurrentState,
            improvement.FutureState, improvement.Source, improvement.ExpectedBenefit,
            improvement.Owner, improvement.Status, improvement.RelatedReviewId,
            improvement.RelatedFindingId
        });

    public async Task<bool> DeleteImprovementAsync(string id) =>
        await _api.DeleteAsync($"api/improvements/{Uri.EscapeDataString(id)}");
}
