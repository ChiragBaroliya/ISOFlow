using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Standards;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class StandardsController : Controller
{
    private readonly IStandardsApiClient _standardsClient;

    public StandardsController(IStandardsApiClient standardsClient)
    {
        _standardsClient = standardsClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Standards";
        var standards = await _standardsClient.GetAllStandardsAsync();
        return View(standards);
    }

    public async Task<IActionResult> Detail(string? id = null)
    {
        ViewData["ActiveMenu"] = "Standards";

        if (string.IsNullOrWhiteSpace(id))
            return RedirectToAction(nameof(Index));

        var standard = await _standardsClient.GetStandardByIdAsync(id);
        if (standard == null)
        {
            var allStandards = await _standardsClient.GetAllStandardsAsync();
            standard = allStandards.FirstOrDefault(s => s.Code.Equals(id, StringComparison.OrdinalIgnoreCase) || s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
        if (standard == null)
            return NotFound($"Standard '{id}' was not found.");

        var reqs = await _standardsClient.GetRequirementsByStandardIdAsync(standard.Id);

        ViewBag.Standard = standard;
        return View(reqs);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Standard standard)
    {
        if (ModelState.IsValid)
        {
            await _standardsClient.CreateStandardAsync(standard);
            TempData["SuccessMessage"] = $"Standard '{standard.Code}' created in database successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Standard standard)
    {
        if (ModelState.IsValid)
        {
            var updated = await _standardsClient.UpdateStandardAsync(standard);
            if (updated != null)
            {
                TempData["SuccessMessage"] = updated.IsPreseeded 
                    ? $"Official Standard '{updated.Code}' compliance target & details updated in database successfully."
                    : $"Standard '{updated.Code}' updated in database successfully.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _standardsClient.DeleteStandardAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Custom Standard deleted from database successfully.";
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
            await _standardsClient.CreateRequirementAsync(requirement.StandardId, requirement);
            TempData["SuccessMessage"] = $"Requirement '{requirement.Clause} - {requirement.Title}' added to database successfully!";
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
            var updated = await _standardsClient.UpdateRequirementAsync(requirement.StandardId, requirement.Id, requirement);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Requirement '{updated.Clause}' updated in database successfully.";
            }
        }
        return RedirectToAction(nameof(Detail), new { id = requirement.StandardId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRequirement(string id, string standardId)
    {
        var result = await _standardsClient.DeleteRequirementAsync(standardId, id);
        if (result)
        {
            TempData["SuccessMessage"] = "Requirement removed from standard in database.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete requirement from database.";
        }
        return RedirectToAction(nameof(Detail), new { id = standardId });
    }
}
