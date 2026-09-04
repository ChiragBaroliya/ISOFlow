using ISOFlow.Api.Auditing;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// Security Controls and Statement of Applicability (SoA) API
/// </summary>
[Authorize]
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
    /// Get paginated and filtered list of Security Controls
    /// </summary>
    /// <param name="request">Pagination and search filter criteria</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<Control>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<Control>>>> GetPaged([FromQuery] PagedRequestDto request)
    {
        var paged = await _controlRepository.GetPagedControlsAsync(request);
        return Ok(ApiResponse<PagedResponse<Control>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all Controls in register
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<Control>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Control>>>> GetAll()
    {
        var controls = await _controlRepository.GetAllControlsAsync();
        return Ok(ApiResponse<List<Control>>.SuccessResponse(controls));
    }

    /// <summary>
    /// Get Control details by ID
    /// </summary>
    /// <param name="id">Control Identifier (e.g. CTRL-001 or A.5.1)</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Control>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Control>), 404)]
    public async Task<ActionResult<ApiResponse<Control>>> GetById(string id)
    {
        var control = await _controlRepository.GetControlByIdAsync(id);
        if (control == null)
            return NotFound(ApiResponse<Control>.FailureResponse($"Control with ID '{id}' was not found."));

        return Ok(ApiResponse<Control>.SuccessResponse(control));
    }

    /// <summary>
    /// Create a new Security Control
    /// </summary>
    [HttpPost]
    [Audit(Module = "Compliance", Entity = "Control", Action = AuditActionType.Create)]
    [ProducesResponseType(typeof(ApiResponse<Control>), 201)]
    public async Task<ActionResult<ApiResponse<Control>>> Create([FromBody] ControlRequestDto dto)
    {
        var entity = new Control
        {
            Code = dto.Code,
            Title = dto.Title,
            RequirementId = dto.RequirementId ?? string.Empty,
            StandardId = dto.StandardId ?? string.Empty,
            Category = dto.Category,
            Description = dto.Description,
            Status = dto.Status,
            Owner = dto.Owner,
            CompliancePercentage = dto.CompliancePercentage,
            IsApplicable = dto.IsApplicable,
            Justification = dto.Justification
        };

        var created = await _controlRepository.CreateControlAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Control>.SuccessResponse(created, "Control created successfully."));
    }

    /// <summary>
    /// Update existing Security Control
    /// </summary>
    [HttpPut("{id}")]
    [Audit(Module = "Compliance", Entity = "Control", Action = AuditActionType.Update)]
    [ProducesResponseType(typeof(ApiResponse<Control>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Control>), 404)]
    public async Task<ActionResult<ApiResponse<Control>>> Update(string id, [FromBody] ControlRequestDto dto)
    {
        var existing = await _controlRepository.GetControlByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse<Control>.FailureResponse($"Control with ID '{id}' was not found."));

        existing.Code = dto.Code;
        existing.Title = dto.Title;
        existing.Category = dto.Category;
        existing.Description = dto.Description;
        existing.Status = dto.Status;
        existing.Owner = dto.Owner;
        existing.CompliancePercentage = dto.CompliancePercentage;
        existing.IsApplicable = dto.IsApplicable;
        existing.Justification = dto.Justification;

        var updated = await _controlRepository.UpdateControlAsync(existing);
        return Ok(ApiResponse<Control>.SuccessResponse(updated!, "Control updated successfully."));
    }

    /// <summary>
    /// Delete a Security Control
    /// </summary>
    [HttpDelete("{id}")]
    [Audit(Module = "Compliance", Entity = "Control", Action = AuditActionType.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var deleted = await _controlRepository.DeleteControlAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Control with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Control deleted successfully."));
    }

    /// <summary>
    /// Get Statement of Applicability (SoA) Matrix
    /// </summary>
    [HttpGet("soa")]
    [ProducesResponseType(typeof(ApiResponse<List<StatementOfApplicability>>), 200)]
    public async Task<ActionResult<ApiResponse<List<StatementOfApplicability>>>> GetSoa()
    {
        var soa = await _controlRepository.GetStatementOfApplicabilityAsync();
        return Ok(ApiResponse<List<StatementOfApplicability>>.SuccessResponse(soa));
    }

    /// <summary>
    /// Get 12-Tab Related Items count breakdown for Control
    /// </summary>
    [HttpGet("{id}/related-items")]
    [ProducesResponseType(typeof(ApiResponse<RelatedItemsCountDto>), 200)]
    public async Task<ActionResult<ApiResponse<RelatedItemsCountDto>>> GetRelatedItems(string id)
    {
        var counts = await _controlRepository.GetRelatedItemsCountAsync(id);
        return Ok(ApiResponse<RelatedItemsCountDto>.SuccessResponse(counts));
    }
}
