using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IManagementReviewRepository
{
    Task<List<ManagementReview>> GetAllReviewsAsync(int? organizationId);
    Task<PagedResponse<ManagementReview>> GetPagedReviewsAsync(PagedRequestDto request, int? organizationId);
    Task<ManagementReview?> GetReviewByIdAsync(string id, int? organizationId);
    Task<ManagementReview> CreateReviewAsync(ManagementReview review);
    Task<ManagementReview?> UpdateReviewAsync(ManagementReview review, int organizationId);
    Task<bool> DeleteReviewAsync(string id, int organizationId);

    Task<List<Improvement>> GetAllImprovementsAsync(int? organizationId);
    Task<PagedResponse<Improvement>> GetPagedImprovementsAsync(PagedRequestDto request, int? organizationId);
    Task<Improvement?> GetImprovementByIdAsync(string id, int? organizationId);
    Task<Improvement> CreateImprovementAsync(Improvement improvement);
    Task<Improvement?> UpdateImprovementAsync(Improvement improvement, int organizationId);
    Task<bool> DeleteImprovementAsync(string id, int organizationId);
}
