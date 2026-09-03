using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.ManagementReviews;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class ManagementReviewController : Controller
{
    private readonly IManagementReviewsApiClient _reviewsClient;

    public ManagementReviewController(IManagementReviewsApiClient reviewsClient)
    {
        _reviewsClient = reviewsClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "ManagementReview";
        ViewData["ActiveTraceabilityId"] = "MR-Q4-2026";
        var reviews = await _reviewsClient.GetAllReviewsAsync();
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
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim())
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToList();
            }
            await _reviewsClient.CreateReviewAsync(review);
            TempData["SuccessMessage"] = $"Management Review '{review.Title}' created successfully!";
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
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim())
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToList();
            }
            var updated = await _reviewsClient.UpdateReviewAsync(review);
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
        var result = await _reviewsClient.DeleteReviewAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Management Review removed.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete management review.";
        }
        return RedirectToAction(nameof(Index));
    }
}
