using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.ManagementReviews;

public interface IManagementReviewsApiClient
{
    Task<List<ManagementReview>> GetAllReviewsAsync();
    Task<ManagementReview?> CreateReviewAsync(ManagementReview review);
    Task<ManagementReview?> UpdateReviewAsync(ManagementReview review);
    Task<bool> DeleteReviewAsync(string id);
}

public class ManagementReviewsApiClient : IManagementReviewsApiClient
{
    private readonly IApiHttpClient _api;

    public ManagementReviewsApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<ManagementReview>> GetAllReviewsAsync() =>
        await _api.GetAsync<List<ManagementReview>>("api/managementreviews/all") ?? new();

    public async Task<ManagementReview?> CreateReviewAsync(ManagementReview review) =>
        await _api.PostAsync<ManagementReview>("api/managementreviews", new
        {
            review.Code, review.Title, review.Period, review.ReviewDate,
            review.ChairPerson, review.Attendees, review.Summary, review.Status
        });

    public async Task<ManagementReview?> UpdateReviewAsync(ManagementReview review) =>
        await _api.PutAsync<ManagementReview>($"api/managementreviews/{Uri.EscapeDataString(review.Id)}", new
        {
            review.Code, review.Title, review.Period, review.ReviewDate,
            review.ChairPerson, review.Attendees, review.Summary, review.Status
        });

    public async Task<bool> DeleteReviewAsync(string id) =>
        await _api.DeleteAsync($"api/managementreviews/{Uri.EscapeDataString(id)}");
}
