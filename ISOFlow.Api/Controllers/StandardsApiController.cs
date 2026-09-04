using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// ISO Compliance Standards and Requirements Register API
/// </summary>
[Authorize]
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
    /// Get paginated and filtered list of ISO Standards
    /// </summary>
    /// <param name="request">Pagination, search and filter parameters</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<Standard>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<Standard>>>> GetPaged([FromQuery] PagedRequestDto request)
    {
        var paged = await _standardRepository.GetPagedStandardsAsync(request);
        return Ok(ApiResponse<PagedResponse<Standard>>.SuccessResponse(paged, "Standards retrieved successfully."));
    }

    /// <summary>
    /// Get all registered ISO Standards
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<Standard>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Standard>>>> GetAll()
    {
        var standards = await _standardRepository.GetAllStandardsAsync();
        return Ok(ApiResponse<List<Standard>>.SuccessResponse(standards));
    }

    /// <summary>
    /// Get ISO Standard details by Identifier
    /// </summary>
    /// <param name="id">Standard Identifier or Code (e.g. ISO-27001-2022)</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Standard>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Standard>), 404)]
    public async Task<ActionResult<ApiResponse<Standard>>> GetById(string id)
    {
        var standard = await _standardRepository.GetStandardByIdAsync(id);
        if (standard == null)
            return NotFound(ApiResponse<Standard>.FailureResponse($"Standard with ID '{id}' was not found."));

        return Ok(ApiResponse<Standard>.SuccessResponse(standard));
    }

    /// <summary>
    /// Register a new ISO standard
    /// </summary>
    /// <param name="dto">Standard creation payload</param>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<Standard>), 201)]
    [ProducesResponseType(typeof(ApiResponse<Standard>), 400)]
    public async Task<ActionResult<ApiResponse<Standard>>> Create([FromBody] StandardRequestDto dto)
    {
        var entity = new Standard
        {
            Code = dto.Code,
            Name = dto.Name,
            Revision = dto.Revision,
            Description = dto.Description,
            CompliancePercentage = dto.CompliancePercentage,
            Status = dto.Status,
            IsPreseeded = false
        };

        var created = await _standardRepository.CreateStandardAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Standard>.SuccessResponse(created, "Standard created successfully."));
    }

    /// <summary>
    /// Update existing ISO standard
    /// </summary>
    /// <param name="id">Standard Identifier</param>
    /// <param name="dto">Standard update payload</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Standard>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Standard>), 404)]
    public async Task<ActionResult<ApiResponse<Standard>>> Update(string id, [FromBody] StandardRequestDto dto)
    {
        var existing = await _standardRepository.GetStandardByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse<Standard>.FailureResponse($"Standard with ID '{id}' was not found."));

        existing.Name = dto.Name;
        existing.Revision = dto.Revision;
        existing.Description = dto.Description;
        existing.CompliancePercentage = dto.CompliancePercentage;

        if (!existing.IsPreseeded)
        {
            existing.Code = dto.Code;
            existing.Status = dto.Status;
        }

        var updated = await _standardRepository.UpdateStandardAsync(existing);
        return Ok(ApiResponse<Standard>.SuccessResponse(updated!, "Standard updated successfully."));
    }

    /// <summary>
    /// Delete a custom ISO standard (Pre-seeded official standards cannot be deleted)
    /// </summary>
    /// <param name="id">Standard Identifier</param>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var existing = await _standardRepository.GetStandardByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse<bool>.FailureResponse($"Standard with ID '{id}' was not found."));

        if (existing.IsPreseeded)
            return BadRequest(ApiResponse<bool>.FailureResponse("Pre-seeded official standards (e.g. ISO 27001) cannot be deleted."));

        var deleted = await _standardRepository.DeleteStandardAsync(id);
        return Ok(ApiResponse<bool>.SuccessResponse(deleted, "Standard deleted successfully."));
    }

    /// <summary>
    /// Get paginated Requirements for a specific ISO Standard
    /// </summary>
    /// <param name="id">Standard Identifier</param>
    /// <param name="request">Pagination and search parameters</param>
    [HttpGet("{id}/requirements")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<Requirement>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<Requirement>>>> GetRequirements(string id, [FromQuery] PagedRequestDto request)
    {
        var paged = await _standardRepository.GetPagedRequirementsByStandardIdAsync(id, request);
        return Ok(ApiResponse<PagedResponse<Requirement>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Add a Requirement to an ISO Standard
    /// </summary>
    /// <param name="id">Standard Identifier</param>
    /// <param name="dto">Requirement payload</param>
    [HttpPost("{id}/requirements")]
    [ProducesResponseType(typeof(ApiResponse<Requirement>), 201)]
    public async Task<ActionResult<ApiResponse<Requirement>>> CreateRequirement(string id, [FromBody] RequirementRequestDto dto)
    {
        var req = new Requirement
        {
            StandardId = id,
            Clause = dto.Clause,
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            CompliancePercentage = dto.CompliancePercentage,
            RelatedControlIds = dto.RelatedControlIds ?? new List<string>()
        };

        var created = await _standardRepository.CreateRequirementAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Requirement>.SuccessResponse(created, "Requirement created successfully."));
    }

    /// <summary>
    /// Update a Requirement
    /// </summary>
    [HttpPut("{id}/requirements/{reqId}")]
    [ProducesResponseType(typeof(ApiResponse<Requirement>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Requirement>), 404)]
    public async Task<ActionResult<ApiResponse<Requirement>>> UpdateRequirement(string id, string reqId, [FromBody] RequirementRequestDto dto)
    {
        var existing = await _standardRepository.GetRequirementByIdAsync(reqId);
        if (existing == null)
            return NotFound(ApiResponse<Requirement>.FailureResponse($"Requirement with ID '{reqId}' was not found."));

        existing.Clause = dto.Clause;
        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.Category = dto.Category;
        existing.CompliancePercentage = dto.CompliancePercentage;
        existing.RelatedControlIds = dto.RelatedControlIds ?? new List<string>();

        var updated = await _standardRepository.UpdateRequirementAsync(existing);
        return Ok(ApiResponse<Requirement>.SuccessResponse(updated!, "Requirement updated successfully."));
    }

    /// <summary>
    /// Delete a Requirement
    /// </summary>
    [HttpDelete("{id}/requirements/{reqId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRequirement(string id, string reqId)
    {
        var deleted = await _standardRepository.DeleteRequirementAsync(reqId);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Requirement with ID '{reqId}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Requirement deleted successfully."));
    }
}
