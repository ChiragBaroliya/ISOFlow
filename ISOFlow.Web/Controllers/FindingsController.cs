using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class FindingsController : Controller
{
    private readonly IAuditRepository _auditRepository;

    public FindingsController(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Findings";
        ViewData["ActiveTraceabilityId"] = "FIND-001";
        var findings = await _auditRepository.GetAllFindingsAsync();
        return View(findings);
    }

    public async Task<IActionResult> Detail(string id = "FIND-001")
    {
        ViewData["ActiveMenu"] = "Findings";
        ViewData["ActiveTraceabilityId"] = id;
        var finding = await _auditRepository.GetFindingByIdAsync(id);
        return View(finding);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Finding finding)
    {
        if (ModelState.IsValid)
        {
            await _auditRepository.CreateFindingAsync(finding);
            TempData["SuccessMessage"] = $"Audit Finding '{finding.Title}' logged successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Finding finding)
    {
        if (ModelState.IsValid)
        {
            var updated = await _auditRepository.UpdateFindingAsync(finding);
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
        var result = await _auditRepository.DeleteFindingAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Finding removed successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete finding.";
        }
        return RedirectToAction(nameof(Index));
    }
}
