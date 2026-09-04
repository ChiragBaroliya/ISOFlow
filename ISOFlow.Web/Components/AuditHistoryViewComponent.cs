using ISOFlow.Web.Services.AuditLogs;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Components;

/// <summary>
/// Drop this on any entity detail page (Risk, Requirement, Control, Policy, ...) as
/// <c>@await Component.InvokeAsync("AuditHistory", new { entityName = "Control", entityId = Model.Id })</c>
/// to render its full change history. <paramref name="entityName"/> must match the value used in
/// the corresponding controller's <c>[Audit(Entity = "...")]</c> attribute.
/// </summary>
public class AuditHistoryViewComponent : ViewComponent
{
    private readonly IAuditLogsApiClient _auditLogsClient;

    public AuditHistoryViewComponent(IAuditLogsApiClient auditLogsClient)
    {
        _auditLogsClient = auditLogsClient;
    }

    public async Task<IViewComponentResult> InvokeAsync(string entityName, string entityId)
    {
        var history = await _auditLogsClient.GetHistoryAsync(entityName, entityId);

        var role = HttpContext.Session.GetString("ActiveUserSystemRole");
        ViewBag.CanViewDetail = role is "SuperAdmin" or "Admin";

        return View(history);
    }
}
