using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IManagementReviewRepository
{
    Task<List<ManagementReview>> GetAllReviewsAsync();
    Task<PagedResponse<ManagementReview>> GetPagedReviewsAsync(PagedRequestDto request);
    Task<ManagementReview?> GetReviewByIdAsync(string id);
    Task<ManagementReview> CreateReviewAsync(ManagementReview review);
    Task<ManagementReview?> UpdateReviewAsync(ManagementReview review);
    Task<bool> DeleteReviewAsync(string id);

    Task<List<Improvement>> GetAllImprovementsAsync();
    Task<PagedResponse<Improvement>> GetPagedImprovementsAsync(PagedRequestDto request);
    Task<Improvement?> GetImprovementByIdAsync(string id);
    Task<Improvement> CreateImprovementAsync(Improvement improvement);
    Task<Improvement?> UpdateImprovementAsync(Improvement improvement);
    Task<bool> DeleteImprovementAsync(string id);
}
