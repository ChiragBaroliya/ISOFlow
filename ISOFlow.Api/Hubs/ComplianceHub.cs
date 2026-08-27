using Microsoft.AspNetCore.SignalR;

namespace ISOFlow.Api.Hubs;

public class ComplianceHub : Hub
{
    public async Task SendNotification(string title, string message, string category)
    {
        await Clients.All.SendAsync("ReceiveNotification", title, message, category, DateTime.UtcNow);
    }

    public async Task UpdateKpiScore(double newScore)
    {
        await Clients.All.SendAsync("KpiUpdated", newScore);
    }

    public async Task TaskStatusChanged(string taskId, string newStatus)
    {
        await Clients.All.SendAsync("TaskUpdated", taskId, newStatus);
    }
}
