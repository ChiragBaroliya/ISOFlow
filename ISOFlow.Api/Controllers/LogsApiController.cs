using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// System Logs and Date-Wise Log Filtering API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LogsApiController : ControllerBase
{
    private readonly ILogService _logService;

    public LogsApiController(ILogService logService)
    {
        _logService = logService;
    }

    /// <summary>
    /// Fetch and filter Serilog rolling logs by Date Range, Level, and Keyword
    /// </summary>
    /// <param name="fromDate">Start date filter (yyyy-MM-dd)</param>
    /// <param name="toDate">End date filter (yyyy-MM-dd)</param>
    /// <param name="logLevel">Log level filter (Information, Warning, Error, Fatal, Debug)</param>
    /// <param name="search">Search keyword query</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<LogEntryDto>>), 200)]
    public async Task<ActionResult<ApiResponse<List<LogEntryDto>>>> GetLogs(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? logLevel,
        [FromQuery] string? search)
    {
        var logs = await _logService.GetLogsAsync(fromDate, toDate, logLevel, search);
        return Ok(new ApiResponse<List<LogEntryDto>>
        {
            Success = true,
            Message = $"Retrieved {logs.Count} log entries",
            Data = logs
        });
    }
}
