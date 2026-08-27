using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class ImprovementController : Controller
{
    private readonly IManagementReviewRepository _reviewRepository;

    public ImprovementController(IManagementReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Improvement";
        ViewData["ActiveTraceabilityId"] = "IMP-001";
        var improvements = await _reviewRepository.GetAllImprovementsAsync();
        return View(improvements);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Improvement improvement)
    {
        if (ModelState.IsValid)
        {
            await _reviewRepository.CreateImprovementAsync(improvement);
            TempData["SuccessMessage"] = $"Continual Improvement '{improvement.Title}' logged successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Improvement improvement)
    {
        if (ModelState.IsValid)
        {
            var updated = await _reviewRepository.UpdateImprovementAsync(improvement);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Improvement initiative '{updated.Code}' updated successfully.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _reviewRepository.DeleteImprovementAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Improvement initiative removed.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete improvement initiative.";
        }
        return RedirectToAction(nameof(Index));
    }
}
