using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetNotificationsAsync(int? organizationId);
    Task<PagedResponse<Notification>> GetPagedNotificationsAsync(PagedRequestDto request, int? organizationId);
}
