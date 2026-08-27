using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// Compliance Risk Register & Risk Treatments API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RisksController : ControllerBase
{
    private readonly IRiskRepository _riskRepository;

    public RisksController(IRiskRepository riskRepository)
    {
        _riskRepository = riskRepository;
    }

    /// <summary>
    /// Get all Risks in register
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<Risk>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Risk>>>> GetAll()
    {
        var risks = await _riskRepository.GetAllRisksAsync();
        return Ok(new ApiResponse<List<Risk>> { Data = risks });
    }

    /// <summary>
    /// Get Risk details by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Risk>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Risk>), 404)]
    public async Task<ActionResult<ApiResponse<Risk>>> GetById(string id)
    {
        var risk = await _riskRepository.GetRiskByIdAsync(id);
        if (risk == null) return NotFound(new ApiResponse<Risk> { Success = false, Message = "Risk not found" });
        return Ok(new ApiResponse<Risk> { Data = risk });
    }

    /// <summary>
    /// Get all Risk Treatment Plans
    /// </summary>
    [HttpGet("treatments")]
    [ProducesResponseType(typeof(ApiResponse<List<RiskTreatment>>), 200)]
    public async Task<ActionResult<ApiResponse<List<RiskTreatment>>>> GetTreatments()
    {
        var treatments = await _riskRepository.GetAllRiskTreatmentsAsync();
        return Ok(new ApiResponse<List<RiskTreatment>> { Data = treatments });
    }
}
