using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Notifications;

public interface INotificationsApiClient
{
    Task<List<Notification>> GetMineAsync(bool unreadOnly = false, int limit = 20);
    Task<int> GetUnreadCountAsync();
    Task<bool> MarkReadAsync(string id);
    Task<bool> MarkAllReadAsync();
}

public class NotificationsApiClient : INotificationsApiClient
{
    private readonly IApiHttpClient _api;

    public NotificationsApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<Notification>> GetMineAsync(bool unreadOnly = false, int limit = 20) =>
        await _api.GetAsync<List<Notification>>($"api/notifications/mine?unreadOnly={unreadOnly.ToString().ToLowerInvariant()}&limit={limit}") ?? new();

    public async Task<int> GetUnreadCountAsync() =>
        await _api.GetAsync<int?>("api/notifications/mine/unread-count") ?? 0;

    public async Task<bool> MarkReadAsync(string id) =>
        await _api.PostBoolAsync($"api/notifications/{Uri.EscapeDataString(id)}/read");

    public async Task<bool> MarkAllReadAsync() =>
        await _api.PostBoolAsync("api/notifications/read-all");
}
