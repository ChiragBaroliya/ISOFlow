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

    public Task<List<ManagementReview>> GetAllReviewsAsync()
    {
        return QueryMappedListAsync("SELECT * FROM sp_management_reviews_get_all()", r => new ManagementReview
        {
            Id = r.id.ToString(),
            Code = (string)r.code,
            Title = (string)r.title,
            Period = (string)r.period,
            ReviewDate = (DateTime)r.review_date,
            ChairPerson = (string)r.chair_person,
            Summary = (string)r.summary ?? string.Empty,
            Status = (string)r.status
        });
    }

    public Task<PagedResponse<ManagementReview>> GetPagedReviewsAsync(PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", string.IsNullOrWhiteSpace(request.StatusFilter) ? null : request.StatusFilter.Trim());

        return QueryPagedAsync(
            "SELECT * FROM sp_management_reviews_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_status)",
            r => new ManagementReview
            {
                Id = r.id.ToString(),
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

    public Task<ManagementReview?> GetReviewByIdAsync(string id)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT id, code, title, period, review_date, chair_person, summary, status FROM management_reviews WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)",
            r => new ManagementReview
            {
                Id = r.id.ToString(),
                Code = (string)r.code,
                Title = (string)r.title,
                Period = (string)r.period,
                ReviewDate = (DateTime)r.review_date,
                ChairPerson = (string)r.chair_person,
                Summary = (string)r.summary ?? string.Empty,
                Status = (string)r.status
            },
            new { id });
    }

    public async Task<ManagementReview> CreateReviewAsync(ManagementReview review)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_code", review.Code);
        parameters.Add("p_title", review.Title);
        parameters.Add("p_period", review.Period);
        parameters.Add("p_review_date", review.ReviewDate);
        parameters.Add("p_chair_person", review.ChairPerson);
        parameters.Add("p_summary", review.Summary);
        parameters.Add("p_status", string.IsNullOrWhiteSpace(review.Status) ? "Completed" : review.Status);

        var insertedId = await QuerySingleAsync<int>("INSERT INTO management_reviews (code, title, period, review_date, chair_person, summary, status) VALUES (@p_code, @p_title, @p_period, @p_review_date, @p_chair_person, @p_summary, @p_status) RETURNING id", parameters);
        review.Id = insertedId.ToString();
        return review;
    }

    public async Task<ManagementReview?> UpdateReviewAsync(ManagementReview review)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", review.Id);
        parameters.Add("p_title", review.Title);
        parameters.Add("p_period", review.Period);
        parameters.Add("p_review_date", review.ReviewDate);
        parameters.Add("p_chair_person", review.ChairPerson);
        parameters.Add("p_summary", review.Summary);
        parameters.Add("p_status", review.Status);

        var rows = await ExecuteAsync("UPDATE management_reviews SET title = @p_title, period = @p_period, review_date = @p_review_date, chair_person = @p_chair_person, summary = @p_summary, status = @p_status, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE id::VARCHAR = @p_id OR LOWER(code) = LOWER(@p_id)", parameters);
        return rows > 0 ? review : null;
    }

    public async Task<bool> DeleteReviewAsync(string id)
    {
        var rows = await ExecuteAsync("DELETE FROM management_reviews WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)", new { id });
        return rows > 0;
    }

    public Task<List<Improvement>> GetAllImprovementsAsync()
    {
        return QueryMappedListAsync(
            "SELECT id, code, title, current_state, future_state, source, expected_benefit, owner, status, related_review_id, related_finding_id FROM improvements ORDER BY id ASC",
            i => new Improvement
            {
                Id = i.id.ToString(),
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
            });
    }

    public Task<PagedResponse<Improvement>> GetPagedImprovementsAsync(PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", int.TryParse(request.StatusFilter, out var st) ? st : (int?)null);

        return QueryPagedAsync(
            "SELECT * FROM sp_improvements_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_status)",
            i => new Improvement
            {
                Id = i.id.ToString(),
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

    public Task<Improvement?> GetImprovementByIdAsync(string id)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT id, code, title, current_state, future_state, source, expected_benefit, owner, status, related_review_id, related_finding_id FROM improvements WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)",
            i => new Improvement
            {
                Id = i.id.ToString(),
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
            new { id });
    }

    public async Task<Improvement> CreateImprovementAsync(Improvement improvement)
    {
        int.TryParse(improvement.RelatedReviewId, out var revId);
        int.TryParse(improvement.RelatedFindingId, out var findId);

        var parameters = new DynamicParameters();
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

        var insertedId = await QuerySingleAsync<int>("INSERT INTO improvements (code, title, current_state, future_state, source, expected_benefit, owner, status, related_review_id, related_finding_id) VALUES (@p_code, @p_title, @p_current_state, @p_future_state, @p_source, @p_expected_benefit, @p_owner, @p_status, @p_related_review_id, @p_related_finding_id) RETURNING id", parameters);
        improvement.Id = insertedId.ToString();
        return improvement;
    }

    public async Task<Improvement?> UpdateImprovementAsync(Improvement improvement)
    {
        int.TryParse(improvement.RelatedReviewId, out var revId);
        int.TryParse(improvement.RelatedFindingId, out var findId);

        var parameters = new DynamicParameters();
        parameters.Add("p_id", improvement.Id);
        parameters.Add("p_title", improvement.Title);
        parameters.Add("p_current_state", improvement.CurrentState);
        parameters.Add("p_future_state", improvement.FutureState);
        parameters.Add("p_source", (int)improvement.Source);
        parameters.Add("p_expected_benefit", improvement.ExpectedBenefit);
        parameters.Add("p_owner", improvement.Owner);
        parameters.Add("p_status", (int)improvement.Status);
        parameters.Add("p_related_review_id", revId > 0 ? (int?)revId : null);
        parameters.Add("p_related_finding_id", findId > 0 ? (int?)findId : null);

        var rows = await ExecuteAsync("UPDATE improvements SET title = @p_title, current_state = @p_current_state, future_state = @p_future_state, source = @p_source, expected_benefit = @p_expected_benefit, owner = @p_owner, status = @p_status, related_review_id = @p_related_review_id, related_finding_id = @p_related_finding_id, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE id::VARCHAR = @p_id OR LOWER(code) = LOWER(@p_id)", parameters);
        return rows > 0 ? improvement : null;
    }

    public async Task<bool> DeleteImprovementAsync(string id)
    {
        var rows = await ExecuteAsync("DELETE FROM improvements WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)", new { id });
        return rows > 0;
    }
}
