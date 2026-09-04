using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetNotificationsAsync();
    Task<PagedResponse<Notification>> GetPagedNotificationsAsync(PagedRequestDto request);
}
