using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class ManagementReviewRepository : BaseRepository, IManagementReviewRepository
{
    public ManagementReviewRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<List<ManagementReview>> GetAllReviewsAsync(int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedListAsync("SELECT * FROM sp_management_reviews_get_all(@p_organization_id)", r => new ManagementReview
        {
            Id = r.id.ToString(),
            OrganizationId = (int)r.organization_id,
            Code = (string)r.code,
            Title = (string)r.title,
            Period = (string)r.period,
            ReviewDate = (DateTime)r.review_date,
            ChairPerson = (string)r.chair_person,
            Summary = (string)r.summary ?? string.Empty,
            Status = (string)r.status
        }, parameters);
    }

    public Task<PagedResponse<ManagementReview>> GetPagedReviewsAsync(PagedRequestDto request, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", string.IsNullOrWhiteSpace(request.StatusFilter) ? null : request.StatusFilter.Trim());

        return QueryPagedAsync(
            "SELECT * FROM sp_management_reviews_get_paged(@p_page_number, @p_page_size, @p_organization_id, @p_search_term, @p_status)",
            r => new ManagementReview
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                Title = (string)r.title,
                Period = (string)r.period,
                ReviewDate = (DateTime)r.review_date,
                ChairPerson = (string)r.chair_person,
                Summary = (string)r.summary ?? string.Empty,
                Status = (string)r.status
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<ManagementReview?> GetReviewByIdAsync(string id, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_management_reviews_get_by_id(@id, @p_organization_id)",
            r => new ManagementReview
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                Title = (string)r.title,
                Period = (string)r.period,
                ReviewDate = (DateTime)r.review_date,
                ChairPerson = (string)r.chair_person,
                Summary = (string)r.summary ?? string.Empty,
                Status = (string)r.status
            },
            parameters);
    }

    public async Task<ManagementReview> CreateReviewAsync(ManagementReview review)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", review.OrganizationId);
        parameters.Add("p_code", review.Code);
        parameters.Add("p_title", review.Title);
        parameters.Add("p_period", review.Period);
        parameters.Add("p_review_date", review.ReviewDate);
        parameters.Add("p_chair_person", review.ChairPerson);
        parameters.Add("p_summary", review.Summary);
        parameters.Add("p_status", string.IsNullOrWhiteSpace(review.Status) ? "Completed" : review.Status);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_management_reviews_create(@p_organization_id, @p_code, @p_title, @p_period, @p_review_date, @p_chair_person, @p_summary, @p_status)", parameters);
        review.Id = insertedId.ToString();
        return review;
    }

    public async Task<ManagementReview?> UpdateReviewAsync(ManagementReview review, int organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", review.Id);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_title", review.Title);
        parameters.Add("p_chair_person", review.ChairPerson);
        parameters.Add("p_summary", review.Summary);
        parameters.Add("p_status", review.Status);

        var updated = await QuerySingleOrDefaultAsync<bool>("SELECT sp_management_reviews_update(@p_id, @p_organization_id, @p_title, @p_chair_person, @p_summary, @p_status)", parameters);
        return updated ? review : null;
    }

    public async Task<bool> DeleteReviewAsync(string id, int organizationId)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_management_reviews_delete(@id, @organizationId)", new { id, organizationId });
    }

    public Task<List<Improvement>> GetAllImprovementsAsync(int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedListAsync(
            "SELECT * FROM sp_improvements_get_all(@p_organization_id)",
            i => new Improvement
            {
                Id = i.id.ToString(),
                OrganizationId = (int)i.organization_id,
                Code = (string)i.code,
                Title = (string)i.title,
                CurrentState = (string)i.current_state ?? string.Empty,
                FutureState = (string)i.future_state ?? string.Empty,
                Source = (ImprovementSource)(int)i.source,
                ExpectedBenefit = (string)i.expected_benefit ?? string.Empty,
                Owner = (string)i.owner,
                Status = (ImprovementStatus)(int)i.status,
                RelatedReviewId = i.related_review_id != null ? i.related_review_id.ToString() : string.Empty,
                RelatedFindingId = i.related_finding_id != null ? i.related_finding_id.ToString() : string.Empty
            },
            parameters);
    }

    public Task<PagedResponse<Improvement>> GetPagedImprovementsAsync(PagedRequestDto request, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", int.TryParse(request.StatusFilter, out var st) ? st : (int?)null);

        return QueryPagedAsync(
            "SELECT * FROM sp_improvements_get_paged(@p_page_number, @p_page_size, @p_organization_id, @p_search_term, @p_status)",
            i => new Improvement
            {
                Id = i.id.ToString(),
                OrganizationId = (int)i.organization_id,
                Code = (string)i.code,
                Title = (string)i.title,
                CurrentState = (string)i.current_state ?? string.Empty,
                FutureState = (string)i.future_state ?? string.Empty,
                Source = (ImprovementSource)(int)i.source,
                ExpectedBenefit = (string)i.expected_benefit ?? string.Empty,
                Owner = (string)i.owner,
                Status = (ImprovementStatus)(int)i.status,
                RelatedReviewId = i.related_review_id != null ? i.related_review_id.ToString() : string.Empty,
                RelatedFindingId = i.related_finding_id != null ? i.related_finding_id.ToString() : string.Empty
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<Improvement?> GetImprovementByIdAsync(string id, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_improvements_get_by_id(@id, @p_organization_id)",
            i => new Improvement
            {
                Id = i.id.ToString(),
                OrganizationId = (int)i.organization_id,
                Code = (string)i.code,
                Title = (string)i.title,
                CurrentState = (string)i.current_state ?? string.Empty,
                FutureState = (string)i.future_state ?? string.Empty,
                Source = (ImprovementSource)(int)i.source,
                ExpectedBenefit = (string)i.expected_benefit ?? string.Empty,
                Owner = (string)i.owner,
                Status = (ImprovementStatus)(int)i.status,
                RelatedReviewId = i.related_review_id != null ? i.related_review_id.ToString() : string.Empty,
                RelatedFindingId = i.related_finding_id != null ? i.related_finding_id.ToString() : string.Empty
            },
            parameters);
    }

    public async Task<Improvement> CreateImprovementAsync(Improvement improvement)
    {
        int.TryParse(improvement.RelatedReviewId, out var revId);
        int.TryParse(improvement.RelatedFindingId, out var findId);

        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", improvement.OrganizationId);
        parameters.Add("p_code", improvement.Code);
        parameters.Add("p_title", improvement.Title);
        parameters.Add("p_current_state", improvement.CurrentState);
        parameters.Add("p_future_state", improvement.FutureState);
        parameters.Add("p_source", (int)improvement.Source);
        parameters.Add("p_expected_benefit", improvement.ExpectedBenefit);
        parameters.Add("p_owner", improvement.Owner);
        parameters.Add("p_status", (int)improvement.Status);
        parameters.Add("p_related_review_id", revId > 0 ? (int?)revId : null);
        parameters.Add("p_related_finding_id", findId > 0 ? (int?)findId : null);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_improvements_create(@p_organization_id, @p_code, @p_title, @p_current_state, @p_future_state, @p_source, @p_expected_benefit, @p_owner, @p_status, @p_related_review_id, @p_related_finding_id)", parameters);
        improvement.Id = insertedId.ToString();
        return improvement;
    }

    public async Task<Improvement?> UpdateImprovementAsync(Improvement improvement, int organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", improvement.Id);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_title", improvement.Title);
        parameters.Add("p_current_state", improvement.CurrentState);
        parameters.Add("p_future_state", improvement.FutureState);
        parameters.Add("p_expected_benefit", improvement.ExpectedBenefit);
        parameters.Add("p_owner", improvement.Owner);
        parameters.Add("p_status", (int)improvement.Status);

        var updated = await QuerySingleOrDefaultAsync<bool>("SELECT sp_improvements_update(@p_id, @p_organization_id, @p_title, @p_current_state, @p_future_state, @p_expected_benefit, @p_owner, @p_status)", parameters);
        return updated ? improvement : null;
    }

    public async Task<bool> DeleteImprovementAsync(string id, int organizationId)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_improvements_delete(@id, @organizationId)", new { id, organizationId });
    }
}
