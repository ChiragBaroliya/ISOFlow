using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Improvements;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class ImprovementController : Controller
{
    private readonly IImprovementsApiClient _improvementsClient;

    public ImprovementController(IImprovementsApiClient improvementsClient)
    {
        _improvementsClient = improvementsClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Improvement";
        ViewData["ActiveTraceabilityId"] = "IMP-001";
        var improvements = await _improvementsClient.GetAllImprovementsAsync();
        return View(improvements);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Improvement improvement)
    {
        if (ModelState.IsValid)
        {
            await _improvementsClient.CreateImprovementAsync(improvement);
            TempData["SuccessMessage"] = $"Improvement '{improvement.Code}' created successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Improvement improvement)
    {
        if (ModelState.IsValid)
        {
            var updated = await _improvementsClient.UpdateImprovementAsync(improvement);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Improvement '{updated.Code}' updated successfully.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _improvementsClient.DeleteImprovementAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Improvement initiative removed.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete improvement.";
        }
        return RedirectToAction(nameof(Index));
    }
}
