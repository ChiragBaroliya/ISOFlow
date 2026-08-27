using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// ISO Compliance Standards Register API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StandardsController : ControllerBase
{
    private readonly IStandardRepository _standardRepository;

    public StandardsController(IStandardRepository standardRepository)
    {
        _standardRepository = standardRepository;
    }

    /// <summary>
    /// Get all registered ISO Standards
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<Standard>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Standard>>>> GetAll()
    {
        var standards = await _standardRepository.GetAllStandardsAsync();
        return Ok(new ApiResponse<List<Standard>> { Data = standards });
    }

    /// <summary>
    /// Get ISO Standard by Identifier
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Standard>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Standard>), 404)]
    public async Task<ActionResult<ApiResponse<Standard>>> GetById(string id)
    {
        var standard = await _standardRepository.GetStandardByIdAsync(id);
        if (standard == null) return NotFound(new ApiResponse<Standard> { Success = false, Message = "Standard not found" });
        return Ok(new ApiResponse<Standard> { Data = standard });
    }

    /// <summary>
    /// Get Requirements for a specific ISO Standard
    /// </summary>
    [HttpGet("{id}/requirements")]
    [ProducesResponseType(typeof(ApiResponse<List<Requirement>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Requirement>>>> GetRequirements(string id)
    {
        var reqs = await _standardRepository.GetRequirementsByStandardIdAsync(id);
        return Ok(new ApiResponse<List<Requirement>> { Data = reqs });
    }
}
