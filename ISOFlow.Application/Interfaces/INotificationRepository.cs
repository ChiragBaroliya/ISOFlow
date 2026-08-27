using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetNotificationsAsync();
    Task<List<AuditLog>> GetAuditLogsAsync();
}
