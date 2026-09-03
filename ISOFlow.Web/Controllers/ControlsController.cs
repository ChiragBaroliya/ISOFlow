using ISOFlow.Domain.Entities;
using ISOFlow.Web.Models;
using ISOFlow.Web.Services.Controls;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class ControlsController : Controller
{
    private readonly IControlsApiClient _controlsClient;

    public ControlsController(IControlsApiClient controlsClient)
    {
        _controlsClient = controlsClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Controls";
        ViewData["ActiveTraceabilityId"] = "CTRL-001";
        var controls = await _controlsClient.GetAllControlsAsync();
        return View(controls);
    }

    public async Task<IActionResult> Detail(string id = "CTRL-001")
    {
        ViewData["ActiveMenu"] = "Controls";
        ViewData["ActiveTraceabilityId"] = id;

        var control = await _controlsClient.GetControlByIdAsync(id) ?? new Control { Id = "CTRL-001", Code = "CTRL-001", Title = "User Access Management" };
        var relatedItems = await _controlsClient.GetRelatedItemsCountAsync(id);

        var vm = new ControlDetailViewModel
        {
            Control = control,
            RelatedItems = relatedItems
        };

        return View(vm);
    }

    public async Task<IActionResult> Soa()
    {
        ViewData["ActiveMenu"] = "SoA";
        ViewData["ActiveTraceabilityId"] = "CTRL-001";
        var soa = await _controlsClient.GetStatementOfApplicabilityAsync();
        return View(soa);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Control control)
    {
        if (ModelState.IsValid)
        {
            await _controlsClient.CreateControlAsync(control);
            TempData["SuccessMessage"] = $"Control '{control.Code}' created successfully!";
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
                TempData["SuccessMessage"] = $"Control '{updated.Code}' updated successfully.";
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
            TempData["SuccessMessage"] = "Control deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete control.";
        }
        return RedirectToAction(nameof(Index));
    }
}
