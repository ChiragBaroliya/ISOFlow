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

    public async Task<IActionResult> Detail(string? id = null)
    {
        ViewData["ActiveMenu"] = "Controls";

        var allControls = await _controlsClient.GetAllControlsAsync();
        Control? control = null;

        if (!string.IsNullOrWhiteSpace(id))
        {
            control = await _controlsClient.GetControlByIdAsync(id) 
                   ?? allControls.FirstOrDefault(c => c.Code.Equals(id, StringComparison.OrdinalIgnoreCase) || c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        control ??= allControls.FirstOrDefault();

        if (control == null)
        {
            return NotFound("No controls found in database.");
        }

        ViewData["ActiveTraceabilityId"] = control.Code;
        var relatedItems = await _controlsClient.GetRelatedItemsCountAsync(control.Id);

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
