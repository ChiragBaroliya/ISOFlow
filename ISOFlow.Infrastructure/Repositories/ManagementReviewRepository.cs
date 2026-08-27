using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.MockData;

namespace ISOFlow.Infrastructure.Repositories;

public class ManagementReviewRepository : IManagementReviewRepository
{
    public Task<List<ManagementReview>> GetAllReviewsAsync() => Task.FromResult(MockStore.ManagementReviews);

    public Task<ManagementReview?> GetReviewByIdAsync(string id) =>
        Task.FromResult(MockStore.ManagementReviews.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || r.Code.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<ManagementReview> CreateReviewAsync(ManagementReview review)
    {
        if (string.IsNullOrWhiteSpace(review.Id))
        {
            review.Id = "REV-2026-" + (MockStore.ManagementReviews.Count + 1).ToString("D3");
        }
        if (string.IsNullOrWhiteSpace(review.Code))
        {
            review.Code = review.Id;
        }
        review.Status = string.IsNullOrWhiteSpace(review.Status) ? "Scheduled" : review.Status;
        MockStore.ManagementReviews.Add(review);
        return Task.FromResult(review);
    }

    public Task<ManagementReview?> UpdateReviewAsync(ManagementReview review)
    {
        var existing = MockStore.ManagementReviews.FirstOrDefault(r => r.Id.Equals(review.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Title = review.Title;
            existing.Period = review.Period;
            existing.ReviewDate = review.ReviewDate;
            existing.ChairPerson = review.ChairPerson;
            existing.Attendees = review.Attendees ?? new List<string>();
            existing.Summary = review.Summary;
            existing.Status = review.Status;
        }
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteReviewAsync(string id)
    {
        var existing = MockStore.ManagementReviews.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            MockStore.ManagementReviews.Remove(existing);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<List<Improvement>> GetAllImprovementsAsync() => Task.FromResult(MockStore.Improvements);

    public Task<Improvement?> GetImprovementByIdAsync(string id) =>
        Task.FromResult(MockStore.Improvements.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || i.Code.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<Improvement> CreateImprovementAsync(Improvement improvement)
    {
        if (string.IsNullOrWhiteSpace(improvement.Id))
        {
            improvement.Id = MockStore.NextId("IMP", MockStore.Improvements.Count);
        }
        if (string.IsNullOrWhiteSpace(improvement.Code))
        {
            improvement.Code = improvement.Id;
        }
        MockStore.Improvements.Add(improvement);
        return Task.FromResult(improvement);
    }

    public Task<Improvement?> UpdateImprovementAsync(Improvement improvement)
    {
        var existing = MockStore.Improvements.FirstOrDefault(i => i.Id.Equals(improvement.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Title = improvement.Title;
            existing.CurrentState = improvement.CurrentState;
            existing.FutureState = improvement.FutureState;
            existing.Source = improvement.Source;
            existing.ExpectedBenefit = improvement.ExpectedBenefit;
            existing.Owner = improvement.Owner;
            existing.Status = improvement.Status;
            existing.RelatedReviewId = improvement.RelatedReviewId;
            existing.RelatedFindingId = improvement.RelatedFindingId;
        }
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteImprovementAsync(string id)
    {
        var existing = MockStore.Improvements.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            MockStore.Improvements.Remove(existing);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
