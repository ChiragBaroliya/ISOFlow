using ISOFlow.Web.Services.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Components;

/// <summary>Header bell: unread count badge plus the five most recent notifications.</summary>
public class NotificationBellViewComponent : ViewComponent
{
    private const int RecentCount = 5;

    private readonly INotificationsApiClient _notificationsClient;

    public NotificationBellViewComponent(INotificationsApiClient notificationsClient)
    {
        _notificationsClient = notificationsClient;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        ViewData["UnreadCount"] = await _notificationsClient.GetUnreadCountAsync();
        var recent = await _notificationsClient.GetMineAsync(unreadOnly: false, limit: RecentCount);
        return View(recent);
    }
}
