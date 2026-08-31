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

    public Task<List<AuditLog>> GetAuditLogsAsync()
    {
        return QueryMappedListAsync(
            "SELECT id, \"timestamp\", \"user\", action, module, entity_id, details FROM audit_logs ORDER BY \"timestamp\" DESC",
            r => new AuditLog
            {
                Id = r.id.ToString(),
                Timestamp = (DateTime)r.timestamp,
                User = (string)r.user,
                Action = (string)r.action,
                Module = (string)r.module,
                EntityId = (string)r.entity_id,
                Details = (string)r.details ?? string.Empty
            });
    }

    public Task<PagedResponse<AuditLog>> GetPagedAuditLogsAsync(PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_module", string.IsNullOrWhiteSpace(request.CategoryFilter) ? null : request.CategoryFilter.Trim());

        return QueryPagedAsync(
            "SELECT * FROM sp_audit_logs_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_module)",
            r => new AuditLog
            {
                Id = r.id.ToString(),
                Timestamp = (DateTime)r.timestamp,
                User = (string)r.user,
                Action = (string)r.action,
                Module = (string)r.module,
                EntityId = (string)r.entity_id,
                Details = (string)r.details ?? string.Empty
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }
}
