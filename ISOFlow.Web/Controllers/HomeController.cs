using ISOFlow.Application.Interfaces;
using ISOFlow.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class HomeController : Controller
{
    private readonly IDashboardService _dashboardService;

    public HomeController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Dashboard";
        ViewData["ActiveTraceabilityId"] = "CTRL-001";

        var kpis = await _dashboardService.GetDashboardKpisAsync();
        var trends = await _dashboardService.GetComplianceTrendsAsync();
        var matrix = await _dashboardService.GetRiskMatrixAsync();

        var viewModel = new DashboardViewModel
        {
            Kpis = kpis,
            Trends = trends,
            RiskMatrix = matrix
        };

        return View(viewModel);
    }
}
