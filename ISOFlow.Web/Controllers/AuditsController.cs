using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Web.Models;
using ISOFlow.Web.Services.Audits;
using ISOFlow.Web.Services.Controls;
using ISOFlow.Web.Services.Evidence;
using ISOFlow.Web.Services.Findings;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class AuditsController : Controller
{
    private readonly IAuditsApiClient _auditsClient;
    private readonly IControlsApiClient _controlsClient;
    private readonly IFindingsApiClient _findingsClient;
    private readonly IEvidenceApiClient _evidenceClient;

    public AuditsController(
        IAuditsApiClient auditsClient,
        IControlsApiClient controlsClient,
        IFindingsApiClient findingsClient,
        IEvidenceApiClient evidenceClient)
    {
        _auditsClient = auditsClient;
        _controlsClient = controlsClient;
        _findingsClient = findingsClient;
        _evidenceClient = evidenceClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Audits";
        var audits = await _auditsClient.GetAllAuditsAsync();
        return View(audits);
    }

    public async Task<IActionResult> Detail(string? id = null)
    {
        ViewData["ActiveMenu"] = "Audits";

        if (string.IsNullOrWhiteSpace(id))
            return RedirectToAction(nameof(Index));

        var audit = await _auditsClient.GetAuditByIdAsync(id);
        if (audit == null)
        {
            var allAudits = await _auditsClient.GetAllAuditsAsync();
            audit = allAudits.FirstOrDefault(a => a.Code.Equals(id, StringComparison.OrdinalIgnoreCase) || a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
        if (audit == null)
            return NotFound($"Audit '{id}' was not found.");

        ViewData["ActiveTraceabilityId"] = audit.Code;

        var controls = await _controlsClient.GetAllControlsAsync();
        var findings = (await _findingsClient.GetAllFindingsAsync()).Where(f => f.AuditId == audit.Id).ToList();
        var evidence = (await _evidenceClient.GetAllEvidenceAsync()).Where(e => e.AuditId == audit.Id).ToList();

        // Checklist entries are stored as control codes or ids; resolve each to its control.
        var checklist = audit.CheckListControlIds.Select(reference =>
        {
            var control = controls.FirstOrDefault(c =>
                c.Code.Equals(reference, StringComparison.OrdinalIgnoreCase) || c.Id.Equals(reference, StringComparison.OrdinalIgnoreCase));
            return new AuditChecklistItemViewModel
            {
                Reference = reference,
                Control = control,
                Findings = control == null ? new() : findings.Where(f => f.ControlId == control.Id).ToList(),
                Evidence = control == null ? new() : evidence.Where(e => e.ControlId == control.Id).ToList()
            };
        }).ToList();

        return View(new AuditDetailViewModel { Audit = audit, Checklist = checklist, Findings = findings });
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
