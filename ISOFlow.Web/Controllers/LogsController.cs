using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class LogsController : Controller
{
    private readonly ILogService _logService;

    public LogsController(ILogService logService)
    {
        _logService = logService;
    }

    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string? logLevel, string? search)
    {
        ViewData["ActiveMenu"] = "Logs";
        ViewData["ActiveTraceabilityId"] = "LOG-HUB";

        var defaultFrom = fromDate ?? DateTime.Today.AddDays(-7);
        var defaultTo = toDate ?? DateTime.Today;

        ViewBag.FromDate = defaultFrom.ToString("yyyy-MM-dd");
        ViewBag.ToDate = defaultTo.ToString("yyyy-MM-dd");
        ViewBag.LogLevel = string.IsNullOrWhiteSpace(logLevel) ? "All" : logLevel;
        ViewBag.Search = search ?? string.Empty;

        var logs = await _logService.GetLogsAsync(defaultFrom, defaultTo, logLevel, search);
        return View(logs);
    }
}
