using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.MockData;

namespace ISOFlow.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    public Task<List<Notification>> GetNotificationsAsync() => Task.FromResult(MockStore.Notifications);

    public Task<List<AuditLog>> GetAuditLogsAsync() => Task.FromResult(MockStore.AuditLogs);
}
