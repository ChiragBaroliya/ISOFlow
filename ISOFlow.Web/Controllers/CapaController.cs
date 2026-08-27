using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class CapaController : Controller
{
    private readonly ICapaRepository _capaRepository;

    public CapaController(ICapaRepository capaRepository)
    {
        _capaRepository = capaRepository;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Capa";
        ViewData["ActiveTraceabilityId"] = "CAPA-001";
        var capas = await _capaRepository.GetAllCapasAsync();
        return View(capas);
    }

    public async Task<IActionResult> Detail(string id = "CAPA-001")
    {
        ViewData["ActiveMenu"] = "Capa";
        ViewData["ActiveTraceabilityId"] = id;
        var capa = await _capaRepository.GetCapaByIdAsync(id);
        return View(capa);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CAPA capa)
    {
        if (ModelState.IsValid)
        {
            await _capaRepository.CreateCapaAsync(capa);
            TempData["SuccessMessage"] = $"CAPA '{capa.Title}' initiated successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CAPA capa)
    {
        if (ModelState.IsValid)
        {
            var updated = await _capaRepository.UpdateCapaAsync(capa);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"CAPA '{updated.Code}' updated successfully.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _capaRepository.DeleteCapaAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "CAPA deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete CAPA.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AddActionItem(string capaId, CapaActionItem item)
    {
        await _capaRepository.AddActionItemAsync(capaId, item);
        TempData["SuccessMessage"] = "Action item added to CAPA checklist.";
        return RedirectToAction(nameof(Detail), new { id = capaId });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAction(string capaId, string actionItemId)
    {
        await _capaRepository.ToggleActionItemAsync(capaId, actionItemId);
        return RedirectToAction(nameof(Detail), new { id = capaId });
    }
}
