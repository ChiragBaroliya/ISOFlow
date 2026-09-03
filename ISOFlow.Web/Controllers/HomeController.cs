using ISOFlow.Web.Models;
using ISOFlow.Web.Services.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class HomeController : Controller
{
    private readonly IDashboardApiClient _dashboardClient;

    public HomeController(IDashboardApiClient dashboardClient)
    {
        _dashboardClient = dashboardClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Dashboard";
        ViewData["ActiveTraceabilityId"] = "CTRL-001";

        var kpis = await _dashboardClient.GetDashboardKpisAsync();
        var trends = await _dashboardClient.GetComplianceTrendsAsync();
        var matrix = await _dashboardClient.GetRiskMatrixAsync();

        var viewModel = new DashboardViewModel
        {
            Kpis = kpis,
            Trends = trends,
            RiskMatrix = matrix
        };

        return View(viewModel);
    }
}
