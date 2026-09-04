using ISOFlow.Application.DTOs;
using ISOFlow.Web.Filters;
using ISOFlow.Web.Services.AuditLogs;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

/// <summary>
/// Global, cross-entity Audit Logs browser. Restricted to SuperAdmin/Admin — an entity's own
/// "History" tab (any authenticated user) is served separately by <see cref="Components.AuditHistoryViewComponent"/>.
/// </summary>
[SessionAuthorize(Roles = "SuperAdmin,Admin")]
public class AuditLogsController : Controller
{
    private readonly IAuditLogsApiClient _auditLogsClient;

    public AuditLogsController(IAuditLogsApiClient auditLogsClient)
    {
        _auditLogsClient = auditLogsClient;
    }

    public async Task<IActionResult> Index(AuditLogFilterDto filter)
    {
        ViewData["Title"] = "Audit Logs — ISOFlow";
        ViewData["ActiveMenu"] = "AuditLogs";

        var paged = await _auditLogsClient.GetPagedAsync(filter);
        ViewBag.Filter = filter;
        return View(paged);
    }

    public async Task<IActionResult> Details(string id)
    {
        ViewData["Title"] = "Audit Log Detail — ISOFlow";
        ViewData["ActiveMenu"] = "AuditLogs";

        var detail = await _auditLogsClient.GetByIdAsync(id);
        if (detail == null)
        {
            TempData["ErrorMessage"] = $"Audit log entry '{id}' was not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(detail);
    }
}
