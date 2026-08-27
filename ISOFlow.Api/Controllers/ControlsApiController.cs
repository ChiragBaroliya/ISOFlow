using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// Security Controls & Statement of Applicability (SoA) API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ControlsController : ControllerBase
{
    private readonly IControlRepository _controlRepository;

    public ControlsController(IControlRepository controlRepository)
    {
        _controlRepository = controlRepository;
    }

    /// <summary>
    /// Get all Controls in register
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<Control>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Control>>>> GetAll()
    {
        var controls = await _controlRepository.GetAllControlsAsync();
        return Ok(new ApiResponse<List<Control>> { Data = controls });
    }

    /// <summary>
    /// Get Control details by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Control>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Control>), 404)]
    public async Task<ActionResult<ApiResponse<Control>>> GetById(string id)
    {
        var control = await _controlRepository.GetControlByIdAsync(id);
        if (control == null) return NotFound(new ApiResponse<Control> { Success = false, Message = "Control not found" });
        return Ok(new ApiResponse<Control> { Data = control });
    }

    /// <summary>
    /// Get Statement of Applicability (SoA) Matrix
    /// </summary>
    [HttpGet("soa")]
    [ProducesResponseType(typeof(ApiResponse<List<StatementOfApplicability>>), 200)]
    public async Task<ActionResult<ApiResponse<List<StatementOfApplicability>>>> GetSoa()
    {
        var soa = await _controlRepository.GetStatementOfApplicabilityAsync();
        return Ok(new ApiResponse<List<StatementOfApplicability>> { Data = soa });
    }

    /// <summary>
    /// Get 12-Tab Related Items count breakdown for Control
    /// </summary>
    [HttpGet("{id}/related-items")]
    [ProducesResponseType(typeof(ApiResponse<RelatedItemsCountDto>), 200)]
    public async Task<ActionResult<ApiResponse<RelatedItemsCountDto>>> GetRelatedItems(string id)
    {
        var counts = await _controlRepository.GetRelatedItemsCountAsync(id);
        return Ok(new ApiResponse<RelatedItemsCountDto> { Data = counts });
    }
}
