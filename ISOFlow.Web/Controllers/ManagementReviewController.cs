using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class ManagementReviewController : Controller
{
    private readonly IManagementReviewRepository _reviewRepository;

    public ManagementReviewController(IManagementReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "ManagementReview";
        ViewData["ActiveTraceabilityId"] = "REV-2026-Q4";
        var reviews = await _reviewRepository.GetAllReviewsAsync();
        return View(reviews);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ManagementReview review, string? attendeesRaw)
    {
        if (ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(attendeesRaw))
            {
                review.Attendees = attendeesRaw
                    .Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim())
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToList();
            }
            await _reviewRepository.CreateReviewAsync(review);
            TempData["SuccessMessage"] = $"Management Review '{review.Title}' scheduled successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ManagementReview review, string? attendeesRaw)
    {
        if (ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(attendeesRaw))
            {
                review.Attendees = attendeesRaw
                    .Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim())
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToList();
            }
            var updated = await _reviewRepository.UpdateReviewAsync(review);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Management Review '{updated.Code}' updated successfully.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _reviewRepository.DeleteReviewAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Management Review deleted.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete review.";
        }
        return RedirectToAction(nameof(Index));
    }
}
