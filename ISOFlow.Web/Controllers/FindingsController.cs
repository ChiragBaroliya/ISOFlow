using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Findings;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class FindingsController : Controller
{
    private readonly IFindingsApiClient _findingsClient;

    public FindingsController(IFindingsApiClient findingsClient)
    {
        _findingsClient = findingsClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Findings";
        ViewData["ActiveTraceabilityId"] = "FIND-001";
        var findings = await _findingsClient.GetAllFindingsAsync();
        return View(findings);
    }

    public async Task<IActionResult> Detail(string? id = null)
    {
        ViewData["ActiveMenu"] = "Findings";

        var allFindings = await _findingsClient.GetAllFindingsAsync();
        Finding? finding = null;

        if (!string.IsNullOrWhiteSpace(id))
        {
            finding = await _findingsClient.GetFindingByIdAsync(id)
                ?? allFindings.FirstOrDefault(f => f.Code.Equals(id, StringComparison.OrdinalIgnoreCase) || f.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        finding ??= allFindings.FirstOrDefault();

        if (finding == null)
        {
            return NotFound("No findings found in database.");
        }

        ViewData["ActiveTraceabilityId"] = finding.Code;
        return View(finding);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Finding finding)
    {
        if (ModelState.IsValid)
        {
            await _findingsClient.CreateFindingAsync(finding);
            TempData["SuccessMessage"] = $"Finding '{finding.Code}' logged in database successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Finding finding)
    {
        if (ModelState.IsValid)
        {
            var updated = await _findingsClient.UpdateFindingAsync(finding);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Finding '{updated.Code}' updated in database.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _findingsClient.DeleteFindingAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Finding removed from database.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete finding from database.";
        }
        return RedirectToAction(nameof(Index));
    }
}
