using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class ReportsController : Controller
{
    public IActionResult Index()
    {
        ViewData["ActiveMenu"] = "Reports";
        ViewData["ActiveTraceabilityId"] = "CTRL-001";
        return View();
    }
}
