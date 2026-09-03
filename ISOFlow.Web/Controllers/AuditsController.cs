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

    public async Task<IActionResult> Detail(string? id = null)
    {
        ViewData["ActiveMenu"] = "Audits";

        var allAudits = await _auditsClient.GetAllAuditsAsync();
        Audit? audit = null;

        if (!string.IsNullOrWhiteSpace(id))
        {
            audit = await _auditsClient.GetAuditByIdAsync(id)
                ?? allAudits.FirstOrDefault(a => a.Code.Equals(id, StringComparison.OrdinalIgnoreCase) || a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        audit ??= allAudits.FirstOrDefault();

        if (audit == null)
        {
            return NotFound("No audits found in database.");
        }

        ViewData["ActiveTraceabilityId"] = audit.Code;
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
            TempData["SuccessMessage"] = $"Audit '{audit.Title}' scheduled in database successfully!";
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
                TempData["SuccessMessage"] = $"Audit '{updated.Code}' updated in database.";
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
            TempData["SuccessMessage"] = "Audit deleted from database.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete audit from database.";
        }
        return RedirectToAction(nameof(Index));
    }
}
