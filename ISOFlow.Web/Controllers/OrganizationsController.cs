using ISOFlow.Domain.Entities;
using ISOFlow.Web.Filters;
using ISOFlow.Web.Services.Organizations;
using ISOFlow.Web.Services.Standards;
using ISOFlow.Web.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

[SessionAuthorize(Roles = "SuperAdmin")]
public class OrganizationsController : Controller
{
    private readonly IOrganizationsApiClient _orgsClient;
    private readonly IUsersApiClient _usersClient;
    private readonly IStandardsApiClient _standardsClient;

    public OrganizationsController(
        IOrganizationsApiClient orgsClient,
        IUsersApiClient usersClient,
        IStandardsApiClient standardsClient)
    {
        _orgsClient = orgsClient;
        _usersClient = usersClient;
        _standardsClient = standardsClient;
    }

    // ── SuperAdmin guard helper ───────────────────────────────────────────────
    private bool IsSuperAdmin() =>
        HttpContext.Session.GetString("ActiveUserSystemRole") == "SuperAdmin";

    public async Task<IActionResult> Index()
    {
        if (!IsSuperAdmin()) return RedirectToAction("AccessDenied", "Account");

        ViewData["ActiveMenu"] = "Organizations";
        ViewData["ActiveTraceabilityId"] = "ORG-001";

        var orgs = await _orgsClient.GetAllOrganizationsAsync();
        var standards = await _standardsClient.GetAllStandardsAsync();

        ViewBag.Standards = standards;
        return View(orgs);
    }

    public async Task<IActionResult> Detail(string id = "ORG-001")
    {
        if (!IsSuperAdmin()) return RedirectToAction("AccessDenied", "Account");

        ViewData["ActiveMenu"] = "Organizations";
        ViewData["ActiveTraceabilityId"] = id;

        var org = await _orgsClient.GetOrganizationByIdAsync(id);
        if (org == null)
            return NotFound();

        var users = await _usersClient.GetUsersByOrganizationIdAsync(id);
        var standards = await _standardsClient.GetAllStandardsAsync();

        ViewBag.Users = users;
        ViewBag.Standards = standards;

        return View(org);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Organization organization, string locationsRaw)
    {
        if (!IsSuperAdmin()) return RedirectToAction("AccessDenied", "Account");

        if (!string.IsNullOrWhiteSpace(locationsRaw))
        {
            organization.Locations = locationsRaw.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(l => l.Trim())
                                                 .Where(l => !string.IsNullOrEmpty(l))
                                                 .ToList();
        }

        await _orgsClient.CreateOrganizationAsync(organization);
        TempData["SuccessMessage"] = $"Organization Tenant '{organization.Name}' registered successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Organization organization, string locationsRaw)
    {
        if (!IsSuperAdmin()) return RedirectToAction("AccessDenied", "Account");

        if (!string.IsNullOrWhiteSpace(locationsRaw))
        {
            organization.Locations = locationsRaw.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(l => l.Trim())
                                                 .Where(l => !string.IsNullOrEmpty(l))
                                                 .ToList();
        }

        var updated = await _orgsClient.UpdateOrganizationAsync(organization);
        if (updated != null)
            TempData["SuccessMessage"] = $"Organization Tenant '{updated.Name}' updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (!IsSuperAdmin()) return RedirectToAction("AccessDenied", "Account");

        var activeOrgId = HttpContext.Session.GetString("ActiveOrgId");
        if (id.Equals(activeOrgId, StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You cannot delete the organization tenant you are currently viewing.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _orgsClient.DeleteOrganizationAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Organization tenant removed.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete organization.";
        }
        return RedirectToAction(nameof(Index));
    }
}
