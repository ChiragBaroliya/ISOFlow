using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// End-to-End Compliance Traceability Graph API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TraceabilityController : ControllerBase
{
    private readonly ITraceabilityService _traceabilityService;

    public TraceabilityController(ITraceabilityService traceabilityService)
    {
        _traceabilityService = traceabilityService;
    }

    /// <summary>
    /// Get Traceability Chain graph rooted at any entity ID (e.g. CTRL-001, RISK-001)
    /// </summary>
    [HttpGet("{entityId}")]
    [ProducesResponseType(typeof(ApiResponse<TraceabilityGraphDto>), 200)]
    public async Task<ActionResult<ApiResponse<TraceabilityGraphDto>>> GetGraph(string entityId)
    {
        var graph = await _traceabilityService.GetTraceabilityChainAsync(entityId);
        return Ok(new ApiResponse<TraceabilityGraphDto> { Data = graph });
    }
}
