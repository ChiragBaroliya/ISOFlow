using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class NotificationRepository : BaseRepository, INotificationRepository
{
    public NotificationRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<List<Notification>> GetNotificationsAsync()
    {
        return QueryMappedListAsync("SELECT * FROM sp_notifications_get_all()", r => new Notification
        {
            Id = r.id.ToString(),
            Title = (string)r.title,
            Message = (string)r.message,
            Category = (string)r.category,
            CreatedAt = (DateTime)r.created_at,
            IsRead = (bool)r.is_read,
            LinkUrl = (string)r.link_url ?? string.Empty
        });
    }

    public Task<PagedResponse<Notification>> GetPagedNotificationsAsync(PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_category", string.IsNullOrWhiteSpace(request.CategoryFilter) ? null : request.CategoryFilter.Trim());

        return QueryPagedAsync(
            "SELECT * FROM sp_notifications_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_category)",
            r => new Notification
            {
                Id = r.id.ToString(),
                Title = (string)r.title,
                Message = (string)r.message,
                Category = (string)r.category,
                CreatedAt = (DateTime)r.created_at,
                IsRead = (bool)r.is_read,
                LinkUrl = (string)r.link_url ?? string.Empty
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }
}
