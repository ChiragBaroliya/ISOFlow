using ISOFlow.Domain.Entities;
using ISOFlow.Web.Models;
using ISOFlow.Web.Services.Audits;
using ISOFlow.Web.Services.Capa;
using ISOFlow.Web.Services.Controls;
using ISOFlow.Web.Services.Findings;
using ISOFlow.Web.Services.Standards;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class FindingsController : Controller
{
    private readonly IFindingsApiClient _findingsClient;
    private readonly IControlsApiClient _controlsClient;
    private readonly IAuditsApiClient _auditsClient;
    private readonly ICapaApiClient _capaClient;
    private readonly IStandardsApiClient _standardsClient;

    public FindingsController(
        IFindingsApiClient findingsClient,
        IControlsApiClient controlsClient,
        IAuditsApiClient auditsClient,
        ICapaApiClient capaClient,
        IStandardsApiClient standardsClient)
    {
        _findingsClient = findingsClient;
        _controlsClient = controlsClient;
        _auditsClient = auditsClient;
        _capaClient = capaClient;
        _standardsClient = standardsClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Findings";
        var findings = await _findingsClient.GetAllFindingsAsync();
        return View(findings);
    }

    public async Task<IActionResult> Detail(string? id = null)
    {
        ViewData["ActiveMenu"] = "Findings";

        if (string.IsNullOrWhiteSpace(id))
            return RedirectToAction(nameof(Index));

        var finding = await _findingsClient.GetFindingByIdAsync(id);
        if (finding == null)
        {
            var allFindings = await _findingsClient.GetAllFindingsAsync();
            finding = allFindings.FirstOrDefault(f => f.Code.Equals(id, StringComparison.OrdinalIgnoreCase) || f.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
        if (finding == null)
            return NotFound($"Finding '{id}' was not found.");

        ViewData["ActiveTraceabilityId"] = finding.Code;

        var control = string.IsNullOrEmpty(finding.ControlId) ? null : await _controlsClient.GetControlByIdAsync(finding.ControlId);
        var audit = string.IsNullOrEmpty(finding.AuditId) ? null : await _auditsClient.GetAuditByIdAsync(finding.AuditId);

        // The requirement lives under a standard: look it up via the audit's or the control's standard.
        Requirement? requirement = null;
        var standardId = !string.IsNullOrEmpty(audit?.StandardId) ? audit.StandardId : control?.StandardId;
        if (!string.IsNullOrEmpty(finding.RequirementId) && !string.IsNullOrEmpty(standardId))
        {
            requirement = (await _standardsClient.GetRequirementsByStandardIdAsync(standardId))
                .FirstOrDefault(r => r.Id == finding.RequirementId);
        }

        var vm = new FindingDetailViewModel
        {
            Finding = finding,
            Control = control,
            Audit = audit,
            Requirement = requirement,
            Capas = (await _capaClient.GetAllCapasAsync()).Where(c => c.FindingId == finding.Id).ToList()
        };
        return View(vm);
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
