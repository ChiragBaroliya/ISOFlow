using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class AuditsController : Controller
{
    private readonly IAuditRepository _auditRepository;

    public AuditsController(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Audits";
        ViewData["ActiveTraceabilityId"] = "AUD-2026-001";
        var audits = await _auditRepository.GetAllAuditsAsync();
        return View(audits);
    }

    public async Task<IActionResult> Detail(string id = "AUD-2026-001")
    {
        ViewData["ActiveMenu"] = "Audits";
        ViewData["ActiveTraceabilityId"] = id;
        var audit = await _auditRepository.GetAuditByIdAsync(id);
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
            await _auditRepository.CreateAuditAsync(audit);
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
            var updated = await _auditRepository.UpdateAuditAsync(audit);
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
        var result = await _auditRepository.DeleteAuditAsync(id);
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
