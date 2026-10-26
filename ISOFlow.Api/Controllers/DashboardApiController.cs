using ISOFlow.Api.Extensions;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// Dashboard Metrics and Compliance KPIs Endpoint
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get executive dashboard KPIs summary
    /// </summary>
    [HttpGet("kpis")]
    [ProducesResponseType(typeof(ApiResponse<DashboardKpiDto>), 200)]
    public async Task<ActionResult<ApiResponse<DashboardKpiDto>>> GetKpis()
    {
        var kpis = await _dashboardService.GetDashboardKpisAsync(User.GetOrganizationIdOrNull());
        return Ok(new ApiResponse<DashboardKpiDto> { Data = kpis });
    }

    /// <summary>
    /// Get compliance score monthly trends
    /// </summary>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(ApiResponse<List<ComplianceTrendDto>>), 200)]
    public async Task<ActionResult<ApiResponse<List<ComplianceTrendDto>>>> GetTrends()
    {
        var trends = await _dashboardService.GetComplianceTrendsAsync(User.GetOrganizationIdOrNull());
        return Ok(new ApiResponse<List<ComplianceTrendDto>> { Data = trends });
    }

    /// <summary>
    /// Get 5x5 risk matrix cell breakdown
    /// </summary>
    [HttpGet("risk-matrix")]
    [ProducesResponseType(typeof(ApiResponse<List<RiskMatrixCellDto>>), 200)]
    public async Task<ActionResult<ApiResponse<List<RiskMatrixCellDto>>>> GetRiskMatrix()
    {
        var matrix = await _dashboardService.GetRiskMatrixAsync(User.GetOrganizationIdOrNull());
        return Ok(new ApiResponse<List<RiskMatrixCellDto>> { Data = matrix });
    }

    /// <summary>
    /// Get the end-to-end traceability chain for a control
    /// </summary>
    /// <param name="controlId">Control code to trace (e.g. CTRL-001)</param>
    [HttpGet("traceability")]
    [ProducesResponseType(typeof(ApiResponse<TraceabilityGraphDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<TraceabilityGraphDto>), 400)]
    public async Task<ActionResult<ApiResponse<TraceabilityGraphDto>>> GetTraceability([FromQuery] string? controlId)
    {
        if (string.IsNullOrWhiteSpace(controlId))
            return BadRequest(ApiResponse<TraceabilityGraphDto>.FailureResponse("The 'controlId' query parameter is required."));

        var graph = await _dashboardService.GetControlTraceabilityAsync(controlId.Trim(), User.GetOrganizationIdOrNull());
        return Ok(new ApiResponse<TraceabilityGraphDto> { Data = graph });
    }
}
