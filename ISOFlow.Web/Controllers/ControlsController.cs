using ISOFlow.Domain.Entities;
using ISOFlow.Web.Models;
using ISOFlow.Web.Services.Audits;
using ISOFlow.Web.Services.Capa;
using ISOFlow.Web.Services.Controls;
using ISOFlow.Web.Services.Documents;
using ISOFlow.Web.Services.Evidence;
using ISOFlow.Web.Services.Findings;
using ISOFlow.Web.Services.Risks;
using ISOFlow.Web.Services.Standards;
using ISOFlow.Web.Services.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class ControlsController : Controller
{
    private readonly IControlsApiClient _controlsClient;
    private readonly IStandardsApiClient _standardsClient;
    private readonly IRisksApiClient _risksClient;
    private readonly IDocumentsApiClient _documentsClient;
    private readonly ITasksApiClient _tasksClient;
    private readonly IEvidenceApiClient _evidenceClient;
    private readonly IAuditsApiClient _auditsClient;
    private readonly IFindingsApiClient _findingsClient;
    private readonly ICapaApiClient _capaClient;

    public ControlsController(
        IControlsApiClient controlsClient,
        IStandardsApiClient standardsClient,
        IRisksApiClient risksClient,
        IDocumentsApiClient documentsClient,
        ITasksApiClient tasksClient,
        IEvidenceApiClient evidenceClient,
        IAuditsApiClient auditsClient,
        IFindingsApiClient findingsClient,
        ICapaApiClient capaClient)
    {
        _controlsClient = controlsClient;
        _standardsClient = standardsClient;
        _risksClient = risksClient;
        _documentsClient = documentsClient;
        _tasksClient = tasksClient;
        _evidenceClient = evidenceClient;
        _auditsClient = auditsClient;
        _findingsClient = findingsClient;
        _capaClient = capaClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Controls";
        var controls = await _controlsClient.GetAllControlsAsync();
        return View(controls);
    }

    public async Task<IActionResult> Detail(string? id = null)
    {
        ViewData["ActiveMenu"] = "Controls";

        if (string.IsNullOrWhiteSpace(id))
            return RedirectToAction(nameof(Index));

        var control = await _controlsClient.GetControlByIdAsync(id);
        if (control == null)
        {
            var allControls = await _controlsClient.GetAllControlsAsync();
            control = allControls.FirstOrDefault(c => c.Code.Equals(id, StringComparison.OrdinalIgnoreCase) || c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
        if (control == null)
            return NotFound($"Control '{id}' was not found.");

        ViewData["ActiveTraceabilityId"] = control.Code;

        // Links are stored either as numeric ids (FK columns) or as codes (JSON link lists); accept both.
        bool IsThisControl(string? reference) =>
            !string.IsNullOrEmpty(reference) &&
            (reference.Equals(control.Id, StringComparison.OrdinalIgnoreCase) || reference.Equals(control.Code, StringComparison.OrdinalIgnoreCase));

        var risks = (await _risksClient.GetAllRisksAsync()).Where(r => IsThisControl(r.ControlId)).ToList();
        var findings = (await _findingsClient.GetAllFindingsAsync()).Where(f => IsThisControl(f.ControlId)).ToList();
        var evidence = (await _evidenceClient.GetAllEvidenceAsync()).Where(e => IsThisControl(e.ControlId)).ToList();
        var findingIds = findings.Select(f => f.Id).ToHashSet();
        var auditIds = findings.Select(f => f.AuditId).Concat(evidence.Select(e => e.AuditId))
            .Where(a => !string.IsNullOrEmpty(a)).ToHashSet();

        var treatments = new Dictionary<string, RiskTreatment>();
        foreach (var risk in risks)
        {
            var treatment = await _risksClient.GetRiskTreatmentByRiskIdAsync(risk.Id);
            if (treatment != null) treatments[risk.Id] = treatment;
        }

        Standard? standard = string.IsNullOrEmpty(control.StandardId) ? null : await _standardsClient.GetStandardByIdAsync(control.StandardId);
        Requirement? requirement = standard == null || string.IsNullOrEmpty(control.RequirementId)
            ? null
            : (await _standardsClient.GetRequirementsByStandardIdAsync(standard.Id)).FirstOrDefault(r => r.Id == control.RequirementId);

        var vm = new ControlDetailViewModel
        {
            Control = control,
            Standard = standard,
            Requirement = requirement,
            Risks = risks,
            Treatments = treatments,
            Policies = (await _documentsClient.GetAllPoliciesAsync()).Where(p => p.LinkedControlIds.Any(IsThisControl)).ToList(),
            Processes = (await _documentsClient.GetAllProcessesAsync()).Where(p => p.ControlIds.Any(IsThisControl)).ToList(),
            Tasks = (await _tasksClient.GetAllTasksAsync()).Where(t => IsThisControl(t.ControlId)).OrderBy(t => t.DueDate).ToList(),
            Evidence = evidence,
            Findings = findings,
            Audits = (await _auditsClient.GetAllAuditsAsync())
                .Where(a => auditIds.Contains(a.Id) || a.CheckListControlIds.Any(IsThisControl)).ToList(),
            Capas = (await _capaClient.GetAllCapasAsync()).Where(c => findingIds.Contains(c.FindingId)).ToList()
        };

        return View(vm);
    }

    public async Task<IActionResult> Soa()
    {
        ViewData["ActiveMenu"] = "SoA";
        var soa = await _controlsClient.GetStatementOfApplicabilityAsync();
        return View(soa);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Control control)
    {
        if (ModelState.IsValid)
        {
            await _controlsClient.CreateControlAsync(control);
            TempData["SuccessMessage"] = $"Control '{control.Code}' created successfully in database!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Control control)
    {
        if (ModelState.IsValid)
        {
            var updated = await _controlsClient.UpdateControlAsync(control);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Control '{updated.Code}' updated successfully in database.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _controlsClient.DeleteControlAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Control deleted from database.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete control from database.";
        }
        return RedirectToAction(nameof(Index));
    }
}
