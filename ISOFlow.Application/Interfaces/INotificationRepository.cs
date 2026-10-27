using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetNotificationsAsync(int? organizationId);
    Task<PagedResponse<Notification>> GetPagedNotificationsAsync(PagedRequestDto request, int? organizationId);

    /// <summary>
    /// Newest-first notifications visible to <paramref name="userId"/>, with <see cref="Notification.IsRead"/> set
    /// from that user's own read state. <paramref name="organizationId"/> null = all organizations (SuperAdmin).
    /// </summary>
    Task<List<Notification>> GetForUserAsync(int? organizationId, int userId, bool unreadOnly, int limit);

    Task<int> GetUnreadCountAsync(int? organizationId, int userId);

    /// <summary>Marks one notification read for one user. False if it doesn't exist in the organization.</summary>
    Task<bool> MarkReadAsync(int notificationId, int? organizationId, int userId);

    /// <summary>Marks every visible notification read for the user; returns how many were newly marked.</summary>
    Task<int> MarkAllReadAsync(int? organizationId, int userId);

    /// <summary>
    /// Creates a notification. With a <paramref name="sourceKey"/>, at most one notification per organization and
    /// key is ever created: returns null when it already exists.
    /// </summary>
    Task<Notification?> CreateAsync(Notification notification, string? sourceKey = null);
}
