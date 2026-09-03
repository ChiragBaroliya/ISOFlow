using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Evidence;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class EvidenceController : Controller
{
    private readonly IEvidenceApiClient _evidenceClient;

    public EvidenceController(IEvidenceApiClient evidenceClient)
    {
        _evidenceClient = evidenceClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Evidence";
        ViewData["ActiveTraceabilityId"] = "EVI-2026-001";
        var evidence = await _evidenceClient.GetAllEvidenceAsync();
        return View(evidence);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Evidence evidence)
    {
        if (ModelState.IsValid)
        {
            await _evidenceClient.CreateEvidenceAsync(evidence);
            TempData["SuccessMessage"] = $"Evidence '{evidence.Name}' logged in vault successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Evidence evidence)
    {
        if (ModelState.IsValid)
        {
            var updated = await _evidenceClient.UpdateEvidenceAsync(evidence);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Evidence '{updated.Code}' updated successfully.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _evidenceClient.DeleteEvidenceAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Evidence record removed.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete evidence.";
        }
        return RedirectToAction(nameof(Index));
    }
}
