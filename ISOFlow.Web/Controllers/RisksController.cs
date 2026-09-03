using ISOFlow.Domain.Entities;
using ISOFlow.Web.Models;
using ISOFlow.Web.Services.Risks;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class RisksController : Controller
{
    private readonly IRisksApiClient _risksClient;

    public RisksController(IRisksApiClient risksClient)
    {
        _risksClient = risksClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Risks";
        ViewData["ActiveTraceabilityId"] = "RISK-001";
        var risks = await _risksClient.GetAllRisksAsync();
        return View(risks);
    }

    public async Task<IActionResult> Detail(string? id = null)
    {
        ViewData["ActiveMenu"] = "Risks";

        var allRisks = await _risksClient.GetAllRisksAsync();
        Risk? risk = null;

        if (!string.IsNullOrWhiteSpace(id))
        {
            risk = await _risksClient.GetRiskByIdAsync(id)
                ?? allRisks.FirstOrDefault(r => r.Code.Equals(id, StringComparison.OrdinalIgnoreCase) || r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        risk ??= allRisks.FirstOrDefault();

        if (risk == null)
        {
            return NotFound("No risks found in database.");
        }

        ViewData["ActiveTraceabilityId"] = risk.Code;
        var treatment = await _risksClient.GetRiskTreatmentByRiskIdAsync(risk.Id);

        var vm = new RiskDetailViewModel
        {
            Risk = risk,
            Treatment = treatment
        };

        return View(vm);
    }

    public async Task<IActionResult> Matrix()
    {
        ViewData["ActiveMenu"] = "RiskMatrix";
        ViewData["ActiveTraceabilityId"] = "RISK-001";
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
