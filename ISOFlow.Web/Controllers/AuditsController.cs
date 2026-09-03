using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Audits;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class AuditsController : Controller
{
    private readonly IAuditsApiClient _auditsClient;

    public AuditsController(IAuditsApiClient auditsClient)
    {
        _auditsClient = auditsClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Audits";
        ViewData["ActiveTraceabilityId"] = "AUD-2026-001";
        var audits = await _auditsClient.GetAllAuditsAsync();
        return View(audits);
    }

    public async Task<IActionResult> Detail(string id = "AUD-2026-001")
    {
        ViewData["ActiveMenu"] = "Audits";
        ViewData["ActiveTraceabilityId"] = id;
        var audit = await _auditsClient.GetAuditByIdAsync(id);
        return View(audit);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Audit audit, string? controlChecklistRaw)
    {
        if (ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(controlChecklistRaw))
            {
                audit.CheckListControlIds = controlChecklistRaw
                    .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();
            }
            await _auditsClient.CreateAuditAsync(audit);
            TempData["SuccessMessage"] = $"Audit '{audit.Title}' scheduled successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Audit audit, string? controlChecklistRaw)
    {
        if (ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(controlChecklistRaw))
            {
                audit.CheckListControlIds = controlChecklistRaw
                    .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();
            }
            var updated = await _auditsClient.UpdateAuditAsync(audit);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Audit '{updated.Code}' updated successfully.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _auditsClient.DeleteAuditAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Audit deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete audit.";
        }
        return RedirectToAction(nameof(Index));
    }
}
