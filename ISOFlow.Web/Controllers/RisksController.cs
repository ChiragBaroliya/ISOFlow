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

    public async Task<IActionResult> Detail(string id = "RISK-001")
    {
        ViewData["ActiveMenu"] = "Risks";
        ViewData["ActiveTraceabilityId"] = id;

        var risk = await _risksClient.GetRiskByIdAsync(id) ?? new Risk { Id = "RISK-001", Code = "RISK-001", Title = "Unauthorized System & Data Access" };
        var treatment = await _risksClient.GetRiskTreatmentByRiskIdAsync(id);

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
            TempData["SuccessMessage"] = $"Risk '{risk.Code}' added to register successfully!";
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
                TempData["SuccessMessage"] = $"Risk '{updated.Code}' updated successfully.";
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
            TempData["SuccessMessage"] = "Risk deleted from register.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete risk.";
        }
        return RedirectToAction(nameof(Index));
    }
}
