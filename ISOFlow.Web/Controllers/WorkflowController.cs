using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class WorkflowController : Controller
{
    public IActionResult Index()
    {
        ViewData["ActiveMenu"] = "Workflow";
        ViewData["Title"] = "ISOFlow End-to-End Application Flow & Live Architecture";
        return View();
    }
}
