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

    public async Task<IActionResult> Detail(string id = "FIND-001")
    {
        ViewData["ActiveMenu"] = "Findings";
        ViewData["ActiveTraceabilityId"] = id;
        var finding = await _findingsClient.GetFindingByIdAsync(id);
        return View(finding);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Finding finding)
    {
        if (ModelState.IsValid)
        {
            await _findingsClient.CreateFindingAsync(finding);
            TempData["SuccessMessage"] = $"Finding '{finding.Code}' logged successfully!";
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
                TempData["SuccessMessage"] = $"Finding '{updated.Code}' updated successfully.";
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
            TempData["SuccessMessage"] = "Finding removed.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete finding.";
        }
        return RedirectToAction(nameof(Index));
    }
}
