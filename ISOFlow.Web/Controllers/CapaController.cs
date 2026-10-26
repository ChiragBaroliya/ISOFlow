using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Capa;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class CapaController : Controller
{
    private readonly ICapaApiClient _capaClient;

    public CapaController(ICapaApiClient capaClient)
    {
        _capaClient = capaClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "CAPA";
        var capas = await _capaClient.GetAllCapasAsync();
        return View(capas);
    }

    public async Task<IActionResult> Detail(string? id = null)
    {
        ViewData["ActiveMenu"] = "CAPA";

        if (string.IsNullOrWhiteSpace(id))
            return RedirectToAction(nameof(Index));

        var capa = await _capaClient.GetCapaByIdAsync(id);
        if (capa == null)
        {
            var allCapas = await _capaClient.GetAllCapasAsync();
            capa = allCapas.FirstOrDefault(c => c.Code.Equals(id, StringComparison.OrdinalIgnoreCase) || c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
        if (capa == null)
            return NotFound($"CAPA '{id}' was not found.");

        ViewData["ActiveTraceabilityId"] = capa.Code;
        return View(capa);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CAPA capa)
    {
        if (ModelState.IsValid)
        {
            await _capaClient.CreateCapaAsync(capa);
            TempData["SuccessMessage"] = $"CAPA '{capa.Code}' created in database successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CAPA capa)
    {
        if (ModelState.IsValid)
        {
            var updated = await _capaClient.UpdateCapaAsync(capa);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"CAPA '{updated.Code}' updated in database.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _capaClient.DeleteCapaAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "CAPA removed from database.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete CAPA from database.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AddActionItem(string capaId, CapaActionItem item)
    {
        if (ModelState.IsValid)
        {
            await _capaClient.AddActionItemAsync(capaId, item);
            TempData["SuccessMessage"] = $"Action item added to CAPA in database.";
        }
        return RedirectToAction(nameof(Detail), new { id = capaId });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActionItem(string capaId, string actionItemId)
    {
        await _capaClient.ToggleActionItemAsync(capaId, actionItemId);
        return RedirectToAction(nameof(Detail), new { id = capaId });
    }
}
