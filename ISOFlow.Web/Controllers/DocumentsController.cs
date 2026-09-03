using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Documents;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class DocumentsController : Controller
{
    private readonly IDocumentsApiClient _documentsClient;

    public DocumentsController(IDocumentsApiClient documentsClient)
    {
        _documentsClient = documentsClient;
    }

    public async Task<IActionResult> Policies()
    {
        ViewData["ActiveMenu"] = "Policies";
        ViewData["ActiveTraceabilityId"] = "POL-001";
        var policies = await _documentsClient.GetAllPoliciesAsync();
        return View(policies);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePolicy(Policy policy, string? linkedControlsRaw)
    {
        if (ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(linkedControlsRaw))
            {
                policy.LinkedControlIds = linkedControlsRaw
                    .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();
            }
            await _documentsClient.CreatePolicyAsync(policy);
            TempData["SuccessMessage"] = $"Policy '{policy.Code}' created in database!";
        }
        return RedirectToAction(nameof(Policies));
    }

    [HttpPost]
    public async Task<IActionResult> EditPolicy(Policy policy, string? linkedControlsRaw)
    {
        if (ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(linkedControlsRaw))
            {
                policy.LinkedControlIds = linkedControlsRaw
                    .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();
            }
            var updated = await _documentsClient.UpdatePolicyAsync(policy);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Policy '{updated.Code}' updated in database.";
            }
        }
        return RedirectToAction(nameof(Policies));
    }

    [HttpPost]
    public async Task<IActionResult> DeletePolicy(string id)
    {
        var result = await _documentsClient.DeletePolicyAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Policy removed from database.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete policy from database.";
        }
        return RedirectToAction(nameof(Policies));
    }

    public async Task<IActionResult> Processes(string? id)
    {
        ViewData["ActiveMenu"] = "Processes";
        var allProcesses = await _documentsClient.GetAllProcessesAsync();

        Process? process = null;
        if (!string.IsNullOrWhiteSpace(id))
        {
            process = await _documentsClient.GetProcessByIdAsync(id)
                ?? allProcesses.FirstOrDefault(p => p.Code.Equals(id, StringComparison.OrdinalIgnoreCase) || p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        process ??= allProcesses.FirstOrDefault();

        ViewData["ActiveTraceabilityId"] = process?.Code ?? "PROC-001";
        ViewBag.AllProcesses = allProcesses;
        ViewBag.Policies = await _documentsClient.GetAllPoliciesAsync();

        return View(process);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProcess(Process process, string stepsRaw)
    {
        if (!string.IsNullOrWhiteSpace(stepsRaw))
        {
            process.Steps = stepsRaw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim())
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .ToList();
        }

        await _documentsClient.CreateProcessAsync(process);
        TempData["SuccessMessage"] = $"Business Process '{process.Title}' created in database.";
        return RedirectToAction(nameof(Processes), new { id = process.Id });
    }

    [HttpPost]
    public async Task<IActionResult> EditProcess(Process process, string stepsRaw)
    {
        if (!string.IsNullOrWhiteSpace(stepsRaw))
        {
            process.Steps = stepsRaw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim())
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .ToList();
        }

        var updated = await _documentsClient.UpdateProcessAsync(process);
        if (updated != null)
        {
            TempData["SuccessMessage"] = $"Business Process '{updated.Title}' updated in database.";
        }
        return RedirectToAction(nameof(Processes), new { id = process.Id });
    }

    [HttpPost]
    public async Task<IActionResult> ArchiveProcess(string id)
    {
        var result = await _documentsClient.ArchiveProcessAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Business Process archived in database.";
        }
        return RedirectToAction(nameof(Processes));
    }
}
