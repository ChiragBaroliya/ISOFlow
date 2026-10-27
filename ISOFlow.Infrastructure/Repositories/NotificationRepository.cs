using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class NotificationRepository : BaseRepository, INotificationRepository
{
    public NotificationRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<List<Notification>> GetNotificationsAsync(int? organizationId)
    {
        return QueryMappedListAsync(
            "SELECT * FROM sp_notifications_get_all(@p_organization_id)",
            r => new Notification
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Title = (string)r.title,
                Message = (string)r.message,
                Category = (string)r.category,
                CreatedAt = (DateTime)r.created_at,
                IsRead = (bool)r.is_read,
                LinkUrl = (string)r.link_url ?? string.Empty
            },
            new { p_organization_id = organizationId });
    }

    public Task<PagedResponse<Notification>> GetPagedNotificationsAsync(PagedRequestDto request, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_category", string.IsNullOrWhiteSpace(request.CategoryFilter) ? null : request.CategoryFilter.Trim());

        return QueryPagedAsync(
            "SELECT * FROM sp_notifications_get_paged(@p_page_number, @p_page_size, @p_organization_id, @p_search_term, @p_category)",
            r => new Notification
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
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

    public Task<List<Notification>> GetForUserAsync(int? organizationId, int userId, bool unreadOnly, int limit)
    {
        return QueryMappedListAsync(
            @"SELECT n.id, n.organization_id, n.title, n.message, n.category, n.created_at, n.link_url,
                     (r.user_id IS NOT NULL) AS user_has_read
              FROM notifications n
              LEFT JOIN notification_reads r ON r.notification_id = n.id AND r.user_id = @userId
              WHERE (@organizationId IS NULL OR n.organization_id = @organizationId)
                AND (NOT @unreadOnly OR r.user_id IS NULL)
              ORDER BY n.created_at DESC, n.id DESC
              LIMIT @limit",
            r => new Notification
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Title = (string)r.title,
                Message = (string)r.message,
                Category = (string)r.category,
                CreatedAt = (DateTime)r.created_at,
                IsRead = (bool)r.user_has_read,
                LinkUrl = (string)r.link_url ?? string.Empty
            },
            new { organizationId, userId, unreadOnly, limit });
    }

    public Task<int> GetUnreadCountAsync(int? organizationId, int userId)
    {
        return QuerySingleAsync<int>(
            @"SELECT COUNT(*)::INT
              FROM notifications n
              WHERE (@organizationId IS NULL OR n.organization_id = @organizationId)
                AND NOT EXISTS (SELECT 1 FROM notification_reads r WHERE r.notification_id = n.id AND r.user_id = @userId)",
            new { organizationId, userId });
    }

    public async Task<bool> MarkReadAsync(int notificationId, int? organizationId, int userId)
    {
        var visible = await QuerySingleAsync<bool>(
            "SELECT EXISTS (SELECT 1 FROM notifications WHERE id = @notificationId AND (@organizationId IS NULL OR organization_id = @organizationId))",
            new { notificationId, organizationId });
        if (!visible) return false;

        await ExecuteAsync(
            "INSERT INTO notification_reads (notification_id, user_id) VALUES (@notificationId, @userId) ON CONFLICT DO NOTHING",
            new { notificationId, userId });
        return true;
    }

    public Task<int> MarkAllReadAsync(int? organizationId, int userId)
    {
        return ExecuteAsync(
            @"INSERT INTO notification_reads (notification_id, user_id)
              SELECT n.id, @userId FROM notifications n
              WHERE (@organizationId IS NULL OR n.organization_id = @organizationId)
              ON CONFLICT DO NOTHING",
            new { organizationId, userId });
    }

    public async Task<Notification?> CreateAsync(Notification notification, string? sourceKey = null)
    {
        var id = await QuerySingleOrDefaultAsync<int?>(
            @"INSERT INTO notifications (organization_id, title, message, category, link_url, source_key)
              VALUES (@OrganizationId, @Title, @Message, @Category, @LinkUrl, @sourceKey)
              ON CONFLICT (organization_id, source_key) WHERE source_key IS NOT NULL DO NOTHING
              RETURNING id",
            new
            {
                notification.OrganizationId,
                notification.Title,
                notification.Message,
                notification.Category,
                LinkUrl = string.IsNullOrWhiteSpace(notification.LinkUrl) ? null : notification.LinkUrl,
                sourceKey
            });
        if (id == null) return null;

        notification.Id = id.Value.ToString();
        return notification;
    }
}
