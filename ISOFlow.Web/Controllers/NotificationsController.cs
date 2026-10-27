using ISOFlow.Web.Filters;
using ISOFlow.Web.Services.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

[SessionAuthorize]
public class NotificationsController : Controller
{
    private const int PageLimit = 100;

    private readonly INotificationsApiClient _notificationsClient;

    public NotificationsController(INotificationsApiClient notificationsClient)
    {
        _notificationsClient = notificationsClient;
    }

    public async Task<IActionResult> Index(bool unreadOnly = false)
    {
        ViewData["ActiveMenu"] = "Notifications";
        ViewBag.UnreadOnly = unreadOnly;
        var notifications = await _notificationsClient.GetMineAsync(unreadOnly, PageLimit);
        return View(notifications);
    }

    /// <summary>Marks the notification read, then follows its link (local links only).</summary>
    public async Task<IActionResult> Open(string id)
    {
        var mine = await _notificationsClient.GetMineAsync(unreadOnly: false, limit: PageLimit);
        var notification = mine.FirstOrDefault(n => n.Id == id);
        if (notification == null)
            return RedirectToAction(nameof(Index));

        await _notificationsClient.MarkReadAsync(id);

        return !string.IsNullOrWhiteSpace(notification.LinkUrl) && Url.IsLocalUrl(notification.LinkUrl)
            ? Redirect(notification.LinkUrl)
            : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        await _notificationsClient.MarkAllReadAsync();
        TempData["SuccessMessage"] = "All notifications marked as read.";
        return RedirectToAction(nameof(Index));
    }
}
