using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class DocumentsController : Controller
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentsController(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<IActionResult> Policies()
    {
        ViewData["ActiveMenu"] = "Policies";
        ViewData["ActiveTraceabilityId"] = "POL-001";
        var policies = await _documentRepository.GetAllPoliciesAsync();
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
            await _documentRepository.CreatePolicyAsync(policy);
            TempData["SuccessMessage"] = $"Policy '{policy.Code}' created successfully!";
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
            var updated = await _documentRepository.UpdatePolicyAsync(policy);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Policy '{updated.Code}' updated successfully.";
            }
        }
        return RedirectToAction(nameof(Policies));
    }

    [HttpPost]
    public async Task<IActionResult> DeletePolicy(string id)
    {
        var result = await _documentRepository.DeletePolicyAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Policy removed successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete policy.";
        }
        return RedirectToAction(nameof(Policies));
    }

    public async Task<IActionResult> Processes(string? id)
    {
        ViewData["ActiveMenu"] = "Processes";
        var allProcesses = await _documentRepository.GetAllProcessesAsync();

        var selectedId = string.IsNullOrWhiteSpace(id) ? "PROC-001" : id;
        var process = await _documentRepository.GetProcessByIdAsync(selectedId)
                      ?? allProcesses.FirstOrDefault()
                      ?? await _documentRepository.GetProcessByIdAsync("PROC-001");

        ViewData["ActiveTraceabilityId"] = process?.Id ?? "PROC-001";
        ViewBag.AllProcesses = allProcesses;
        ViewBag.Policies = await _documentRepository.GetAllPoliciesAsync();

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

        await _documentRepository.CreateProcessAsync(process);
        TempData["SuccessMessage"] = $"Business Process '{process.Title}' created successfully.";
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

        var updated = await _documentRepository.UpdateProcessAsync(process);
        if (updated != null)
        {
            TempData["SuccessMessage"] = $"Business Process '{updated.Title}' updated successfully.";
        }
        return RedirectToAction(nameof(Processes), new { id = process.Id });
    }

    [HttpPost]
    public async Task<IActionResult> ArchiveProcess(string id)
    {
        var result = await _documentRepository.ArchiveProcessAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Business Process archived successfully.";
        }
        return RedirectToAction(nameof(Processes));
    }
}
