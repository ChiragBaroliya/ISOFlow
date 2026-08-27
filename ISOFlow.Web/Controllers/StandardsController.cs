using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class StandardsController : Controller
{
    private readonly IStandardRepository _standardRepository;

    public StandardsController(IStandardRepository standardRepository)
    {
        _standardRepository = standardRepository;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Standards";
        ViewData["ActiveTraceabilityId"] = "ISO-27001-2022";
        var standards = await _standardRepository.GetAllStandardsAsync();
        return View(standards);
    }

    public async Task<IActionResult> Detail(string id = "ISO-27001-2022")
    {
        ViewData["ActiveMenu"] = "Standards";
        ViewData["ActiveTraceabilityId"] = "REQ-A5-18";
        var standard = await _standardRepository.GetStandardByIdAsync(id);
        var reqs = await _standardRepository.GetRequirementsByStandardIdAsync(id);

        ViewBag.Standard = standard;
        return View(reqs);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Standard standard)
    {
        if (ModelState.IsValid)
        {
            await _standardRepository.CreateStandardAsync(standard);
            TempData["SuccessMessage"] = $"Standard '{standard.Code}' created successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Standard standard)
    {
        if (ModelState.IsValid)
        {
            var updated = await _standardRepository.UpdateStandardAsync(standard);
            if (updated != null)
            {
                TempData["SuccessMessage"] = updated.IsPreseeded 
                    ? $"Official Standard '{updated.Code}' compliance target & details updated successfully."
                    : $"Standard '{updated.Code}' updated successfully.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _standardRepository.DeleteStandardAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Custom Standard deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Official pre-seeded ISO standards are protected and cannot be deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequirement(Requirement requirement, string? linkedControlsRaw)
    {
        if (ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(linkedControlsRaw))
            {
                requirement.RelatedControlIds = linkedControlsRaw
                    .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();
            }
            await _standardRepository.CreateRequirementAsync(requirement);
            TempData["SuccessMessage"] = $"Requirement '{requirement.Clause} - {requirement.Title}' added successfully!";
        }
        return RedirectToAction(nameof(Detail), new { id = requirement.StandardId });
    }

    [HttpPost]
    public async Task<IActionResult> EditRequirement(Requirement requirement, string? linkedControlsRaw)
    {
        if (ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(linkedControlsRaw))
            {
                requirement.RelatedControlIds = linkedControlsRaw
                    .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();
            }
            var updated = await _standardRepository.UpdateRequirementAsync(requirement);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Requirement '{updated.Clause}' updated successfully.";
            }
        }
        return RedirectToAction(nameof(Detail), new { id = requirement.StandardId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRequirement(string id, string standardId)
    {
        var result = await _standardRepository.DeleteRequirementAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Requirement removed from standard.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete requirement.";
        }
        return RedirectToAction(nameof(Detail), new { id = standardId });
    }
}
