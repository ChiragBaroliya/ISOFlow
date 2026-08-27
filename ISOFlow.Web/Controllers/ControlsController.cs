using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class ControlsController : Controller
{
    private readonly IControlRepository _controlRepository;

    public ControlsController(IControlRepository controlRepository)
    {
        _controlRepository = controlRepository;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Controls";
        ViewData["ActiveTraceabilityId"] = "CTRL-001";
        var controls = await _controlRepository.GetAllControlsAsync();
        return View(controls);
    }

    public async Task<IActionResult> Detail(string id = "CTRL-001")
    {
        ViewData["ActiveMenu"] = "Controls";
        ViewData["ActiveTraceabilityId"] = id;

        var control = await _controlRepository.GetControlByIdAsync(id) ?? new Control { Id = "CTRL-001", Code = "CTRL-001", Title = "User Access Management" };
        var relatedItems = await _controlRepository.GetRelatedItemsCountAsync(id);

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
        var soa = await _controlRepository.GetStatementOfApplicabilityAsync();
        return View(soa);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Control control)
    {
        if (ModelState.IsValid)
        {
            await _controlRepository.CreateControlAsync(control);
            TempData["SuccessMessage"] = $"Control '{control.Code}' created successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Control control)
    {
        if (ModelState.IsValid)
        {
            var updated = await _controlRepository.UpdateControlAsync(control);
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
        var result = await _controlRepository.DeleteControlAsync(id);
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
