using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class OrganizationsController : Controller
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStandardRepository _standardRepository;

    public OrganizationsController(
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository,
        IStandardRepository standardRepository)
    {
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
        _standardRepository = standardRepository;
    }

    // ── SuperAdmin guard helper ───────────────────────────────────────────────
    private bool IsSuperAdmin() =>
        HttpContext.Session.GetString("ActiveUserSystemRole") == "SuperAdmin";

    public async Task<IActionResult> Index()
    {
        if (!IsSuperAdmin()) return RedirectToAction("AccessDenied", "Account");

        ViewData["ActiveMenu"] = "Organizations";
        ViewData["ActiveTraceabilityId"] = "ORG-001";

        var orgs = await _organizationRepository.GetAllOrganizationsAsync();
        var standards = await _standardRepository.GetAllStandardsAsync();

        ViewBag.Standards = standards;
        return View(orgs);
    }

    public async Task<IActionResult> Detail(string id = "ORG-001")
    {
        if (!IsSuperAdmin()) return RedirectToAction("AccessDenied", "Account");

        ViewData["ActiveMenu"] = "Organizations";
        ViewData["ActiveTraceabilityId"] = id;

        var org = await _organizationRepository.GetOrganizationByIdAsync(id);
        if (org == null)
            return NotFound();

        var users = await _userRepository.GetUsersByOrganizationIdAsync(id);
        var standards = await _standardRepository.GetAllStandardsAsync();

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

        await _organizationRepository.CreateOrganizationAsync(organization);
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

        var updated = await _organizationRepository.UpdateOrganizationAsync(organization);
        if (updated != null)
            TempData["SuccessMessage"] = $"Organization Tenant '{updated.Name}' updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (!IsSuperAdmin()) return RedirectToAction("AccessDenied", "Account");

        if (id.Equals("ORG-001", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "The primary Organization Tenant (Acme Technologies) is protected and cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _organizationRepository.DeleteOrganizationAsync(id);
        if (result)
            TempData["SuccessMessage"] = "Organization Tenant deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}
