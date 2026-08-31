using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// Corrective and Preventive Actions (CAPA) Management API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CapaController : ControllerBase
{
    private readonly ICapaRepository _capaRepository;

    public CapaController(ICapaRepository capaRepository)
    {
        _capaRepository = capaRepository;
    }

    /// <summary>
    /// Get paginated and filtered list of CAPA items
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CAPA>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<CAPA>>>> GetPaged([FromQuery] PagedRequestDto request)
    {
        var paged = await _capaRepository.GetPagedCapasAsync(request);
        return Ok(ApiResponse<PagedResponse<CAPA>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all CAPA items
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<CAPA>>), 200)]
    public async Task<ActionResult<ApiResponse<List<CAPA>>>> GetAll()
    {
        var capas = await _capaRepository.GetAllCapasAsync();
        return Ok(ApiResponse<List<CAPA>>.SuccessResponse(capas));
    }

    /// <summary>
    /// Get CAPA details by Identifier
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CAPA>), 200)]
    [ProducesResponseType(typeof(ApiResponse<CAPA>), 404)]
    public async Task<ActionResult<ApiResponse<CAPA>>> GetById(string id)
    {
        var capa = await _capaRepository.GetCapaByIdAsync(id);
        if (capa == null)
            return NotFound(ApiResponse<CAPA>.FailureResponse($"CAPA with ID '{id}' was not found."));

        return Ok(ApiResponse<CAPA>.SuccessResponse(capa));
    }

    /// <summary>
    /// Create a new CAPA workflow
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CAPA>), 201)]
    public async Task<ActionResult<ApiResponse<CAPA>>> Create([FromBody] CapaRequestDto dto)
    {
        var capa = new CAPA
        {
            Code = dto.Code,
            FindingId = dto.FindingId ?? string.Empty,
            Title = dto.Title,
            RootCause = dto.RootCause,
            CorrectiveAction = dto.CorrectiveAction,
            Owner = dto.Owner,
            DueDate = dto.DueDate,
            Status = dto.Status,
            EffectivenessVerification = dto.EffectivenessVerification
        };

        var created = await _capaRepository.CreateCapaAsync(capa);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<CAPA>.SuccessResponse(created, "CAPA created successfully."));
    }

    /// <summary>
    /// Update existing CAPA workflow
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CAPA>), 200)]
    [ProducesResponseType(typeof(ApiResponse<CAPA>), 404)]
    public async Task<ActionResult<ApiResponse<CAPA>>> Update(string id, [FromBody] CapaRequestDto dto)
    {
        var existing = await _capaRepository.GetCapaByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse<CAPA>.FailureResponse($"CAPA with ID '{id}' was not found."));

        existing.Title = dto.Title;
        existing.FindingId = dto.FindingId ?? string.Empty;
        existing.RootCause = dto.RootCause;
        existing.CorrectiveAction = dto.CorrectiveAction;
        existing.Owner = dto.Owner;
        existing.DueDate = dto.DueDate;
        existing.Status = dto.Status;
        existing.EffectivenessVerification = dto.EffectivenessVerification;

        var updated = await _capaRepository.UpdateCapaAsync(existing);
        return Ok(ApiResponse<CAPA>.SuccessResponse(updated!, "CAPA updated successfully."));
    }

    /// <summary>
    /// Delete a CAPA record
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var deleted = await _capaRepository.DeleteCapaAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"CAPA with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "CAPA deleted successfully."));
    }

    /// <summary>
    /// Add Action Item to CAPA
    /// </summary>
    [HttpPost("{id}/action-items")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> AddActionItem(string id, [FromBody] CapaActionItemRequestDto dto)
    {
        var item = new CapaActionItem
        {
            Title = dto.Title,
            AssignedTo = dto.AssignedTo,
            DueDate = dto.DueDate,
            IsCompleted = dto.IsCompleted
        };

        var added = await _capaRepository.AddActionItemAsync(id, item);
        if (!added)
            return NotFound(ApiResponse<bool>.FailureResponse($"CAPA with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Action item added successfully."));
    }

    /// <summary>
    /// Toggle Action Item completion status
    /// </summary>
    [HttpPatch("{id}/action-items/{actionItemId}/toggle")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleActionItem(string id, string actionItemId)
    {
        var toggled = await _capaRepository.ToggleActionItemAsync(id, actionItemId);
        if (!toggled)
            return NotFound(ApiResponse<bool>.FailureResponse("CAPA or Action Item was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Action item status toggled successfully."));
    }
}
