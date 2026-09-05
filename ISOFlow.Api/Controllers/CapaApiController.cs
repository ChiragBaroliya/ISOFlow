using ISOFlow.Api.Auditing;
using ISOFlow.Api.Extensions;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// Corrective and Preventive Actions (CAPA) Management API
/// </summary>
[Authorize]
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
        var organizationId = User.GetOrganizationIdOrNull();
        var paged = await _capaRepository.GetPagedCapasAsync(request, organizationId);
        return Ok(ApiResponse<PagedResponse<CAPA>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all CAPA items
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<CAPA>>), 200)]
    public async Task<ActionResult<ApiResponse<List<CAPA>>>> GetAll()
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var capas = await _capaRepository.GetAllCapasAsync(organizationId);
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
        var organizationId = User.GetOrganizationIdOrNull();
        var capa = await _capaRepository.GetCapaByIdAsync(id, organizationId);
        if (capa == null)
            return NotFound(ApiResponse<CAPA>.FailureResponse($"CAPA with ID '{id}' was not found."));

        return Ok(ApiResponse<CAPA>.SuccessResponse(capa));
    }

    /// <summary>
    /// Create a new CAPA workflow
    /// </summary>
    [HttpPost]
    [Audit(Module = "CAPA", Entity = "Capa", Action = AuditActionType.Create)]
    [ProducesResponseType(typeof(ApiResponse<CAPA>), 201)]
    public async Task<ActionResult<ApiResponse<CAPA>>> Create([FromBody] CapaRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<CAPA>.FailureResponse("A specific organization context is required to create this record."));

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
            EffectivenessVerification = dto.EffectivenessVerification,
            OrganizationId = organizationId.Value
        };

        var created = await _capaRepository.CreateCapaAsync(capa);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<CAPA>.SuccessResponse(created, "CAPA created successfully."));
    }

    /// <summary>
    /// Update existing CAPA workflow
    /// </summary>
    [HttpPut("{id}")]
    [Audit(Module = "CAPA", Entity = "Capa", Action = AuditActionType.Update)]
    [ProducesResponseType(typeof(ApiResponse<CAPA>), 200)]
    [ProducesResponseType(typeof(ApiResponse<CAPA>), 404)]
    public async Task<ActionResult<ApiResponse<CAPA>>> Update(string id, [FromBody] CapaRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<CAPA>.FailureResponse("A specific organization context is required to update this record."));

        var existing = await _capaRepository.GetCapaByIdAsync(id, organizationId);
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

        var updated = await _capaRepository.UpdateCapaAsync(existing, organizationId.Value);
        return Ok(ApiResponse<CAPA>.SuccessResponse(updated!, "CAPA updated successfully."));
    }

    /// <summary>
    /// Delete a CAPA record
    /// </summary>
    [HttpDelete("{id}")]
    [Audit(Module = "CAPA", Entity = "Capa", Action = AuditActionType.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<bool>.FailureResponse("A specific organization context is required to delete this record."));

        var deleted = await _capaRepository.DeleteCapaAsync(id, organizationId.Value);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"CAPA with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "CAPA deleted successfully."));
    }

    /// <summary>
    /// Add Action Item to CAPA
    /// </summary>
    [HttpPost("{id}/action-items")]
    [Audit(Module = "CAPA", Entity = "Capa", Action = AuditActionType.Update)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> AddActionItem(string id, [FromBody] CapaActionItemRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<bool>.FailureResponse("A specific organization context is required to add an action item."));

        var item = new CapaActionItem
        {
            Title = dto.Title,
            AssignedTo = dto.AssignedTo,
            DueDate = dto.DueDate,
            IsCompleted = dto.IsCompleted
        };

        var added = await _capaRepository.AddActionItemAsync(id, item, organizationId.Value);
        if (!added)
            return NotFound(ApiResponse<bool>.FailureResponse($"CAPA with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Action item added successfully."));
    }

    /// <summary>
    /// Toggle Action Item completion status
    /// </summary>
    [HttpPatch("{id}/action-items/{actionItemId}/toggle")]
    [Audit(Module = "CAPA", Entity = "Capa", Action = AuditActionType.StatusChange)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleActionItem(string id, string actionItemId)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<bool>.FailureResponse("A specific organization context is required to toggle an action item."));

        var toggled = await _capaRepository.ToggleActionItemAsync(id, actionItemId, organizationId.Value);
        if (!toggled)
            return NotFound(ApiResponse<bool>.FailureResponse("CAPA or Action Item was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Action item status toggled successfully."));
    }
}
