using ISOFlow.Domain.Entities;
using ISOFlow.Web.Models;
using ISOFlow.Web.Services.Controls;
using ISOFlow.Web.Services.Risks;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class RisksController : Controller
{
    private readonly IRisksApiClient _risksClient;
    private readonly IControlsApiClient _controlsClient;

    public RisksController(IRisksApiClient risksClient, IControlsApiClient controlsClient)
    {
        _risksClient = risksClient;
        _controlsClient = controlsClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Risks";
        var risks = await _risksClient.GetAllRisksAsync();
        return View(risks);
    }

    public async Task<IActionResult> Detail(string? id = null)
    {
        ViewData["ActiveMenu"] = "Risks";

        if (string.IsNullOrWhiteSpace(id))
            return RedirectToAction(nameof(Index));

        var risk = await _risksClient.GetRiskByIdAsync(id);
        if (risk == null)
        {
            var allRisks = await _risksClient.GetAllRisksAsync();
            risk = allRisks.FirstOrDefault(r => r.Code.Equals(id, StringComparison.OrdinalIgnoreCase) || r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
        if (risk == null)
            return NotFound($"Risk '{id}' was not found.");

        ViewData["ActiveTraceabilityId"] = risk.Code;
        var treatment = await _risksClient.GetRiskTreatmentByRiskIdAsync(risk.Id);
        var control = string.IsNullOrEmpty(risk.ControlId) ? null : await _controlsClient.GetControlByIdAsync(risk.ControlId);

        var vm = new RiskDetailViewModel
        {
            Risk = risk,
            Control = control,
            Treatment = treatment
        };

        return View(vm);
    }

    public async Task<IActionResult> Matrix()
    {
        ViewData["ActiveMenu"] = "RiskMatrix";
        var matrix = await _risksClient.GetRiskMatrixDataAsync();
        return View(matrix);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Risk risk, RiskTreatment? treatment)
    {
        if (ModelState.IsValid)
        {
            await _risksClient.CreateRiskAsync(risk, treatment);
            TempData["SuccessMessage"] = $"Risk '{risk.Code}' added to register in database!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Risk risk, RiskTreatment? treatment)
    {
        if (ModelState.IsValid)
        {
            var updated = await _risksClient.UpdateRiskAsync(risk, treatment);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Risk '{updated.Code}' updated in database.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _risksClient.DeleteRiskAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Risk deleted from database.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete risk from database.";
        }
        return RedirectToAction(nameof(Index));
    }
}
