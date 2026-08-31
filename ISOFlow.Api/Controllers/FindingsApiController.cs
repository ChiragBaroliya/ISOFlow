using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// Audit Findings and Non-Conformities Management API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FindingsController : ControllerBase
{
    private readonly IAuditRepository _auditRepository;

    public FindingsController(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    /// <summary>
    /// Get paginated and filtered list of Findings
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<Finding>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<Finding>>>> GetPaged([FromQuery] PagedRequestDto request)
    {
        var paged = await _auditRepository.GetPagedFindingsAsync(request);
        return Ok(ApiResponse<PagedResponse<Finding>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all Findings
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<Finding>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Finding>>>> GetAll()
    {
        var findings = await _auditRepository.GetAllFindingsAsync();
        return Ok(ApiResponse<List<Finding>>.SuccessResponse(findings));
    }

    /// <summary>
    /// Get Finding details by Identifier
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Finding>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Finding>), 404)]
    public async Task<ActionResult<ApiResponse<Finding>>> GetById(string id)
    {
        var finding = await _auditRepository.GetFindingByIdAsync(id);
        if (finding == null)
            return NotFound(ApiResponse<Finding>.FailureResponse($"Finding with ID '{id}' was not found."));

        return Ok(ApiResponse<Finding>.SuccessResponse(finding));
    }

    /// <summary>
    /// Log a new Audit Finding
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Finding>), 201)]
    public async Task<ActionResult<ApiResponse<Finding>>> Create([FromBody] FindingRequestDto dto)
    {
        var finding = new Finding
        {
            Code = dto.Code,
            Title = dto.Title,
            AuditId = dto.AuditId ?? string.Empty,
            RequirementId = dto.RequirementId ?? string.Empty,
            ControlId = dto.ControlId ?? string.Empty,
            Severity = dto.Severity,
            Status = dto.Status,
            Description = dto.Description,
            RootCause = dto.RootCause,
            IdentifiedDate = dto.IdentifiedDate,
            Auditor = dto.Auditor,
            CapaId = dto.CapaId ?? string.Empty
        };

        var created = await _auditRepository.CreateFindingAsync(finding);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Finding>.SuccessResponse(created, "Finding created successfully."));
    }

    /// <summary>
    /// Update existing Finding
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Finding>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Finding>), 404)]
    public async Task<ActionResult<ApiResponse<Finding>>> Update(string id, [FromBody] FindingRequestDto dto)
    {
        var existing = await _auditRepository.GetFindingByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse<Finding>.FailureResponse($"Finding with ID '{id}' was not found."));

        existing.Title = dto.Title;
        existing.AuditId = dto.AuditId ?? string.Empty;
        existing.RequirementId = dto.RequirementId ?? string.Empty;
        existing.ControlId = dto.ControlId ?? string.Empty;
        existing.Severity = dto.Severity;
        existing.Status = dto.Status;
        existing.Description = dto.Description;
        existing.RootCause = dto.RootCause;
        existing.Auditor = dto.Auditor;
        existing.CapaId = dto.CapaId ?? string.Empty;

        var updated = await _auditRepository.UpdateFindingAsync(existing);
        return Ok(ApiResponse<Finding>.SuccessResponse(updated!, "Finding updated successfully."));
    }

    /// <summary>
    /// Delete a Finding
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var deleted = await _auditRepository.DeleteFindingAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Finding with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Finding deleted successfully."));
    }
}
