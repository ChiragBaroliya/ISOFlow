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
/// Audit Findings and Non-Conformities Management API
/// </summary>
[Authorize]
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
        var organizationId = User.GetOrganizationIdOrNull();
        var paged = await _auditRepository.GetPagedFindingsAsync(request, organizationId);
        return Ok(ApiResponse<PagedResponse<Finding>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all Findings
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<Finding>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Finding>>>> GetAll()
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var findings = await _auditRepository.GetAllFindingsAsync(organizationId);
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
        var organizationId = User.GetOrganizationIdOrNull();
        var finding = await _auditRepository.GetFindingByIdAsync(id, organizationId);
        if (finding == null)
            return NotFound(ApiResponse<Finding>.FailureResponse($"Finding with ID '{id}' was not found."));

        return Ok(ApiResponse<Finding>.SuccessResponse(finding));
    }

    /// <summary>
    /// Log a new Audit Finding
    /// </summary>
    [HttpPost]
    [Audit(Module = "Audits", Entity = "Finding", Action = AuditActionType.Create)]
    [ProducesResponseType(typeof(ApiResponse<Finding>), 201)]
    public async Task<ActionResult<ApiResponse<Finding>>> Create([FromBody] FindingRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<Finding>.FailureResponse("A specific organization context is required to create this record."));

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
            CapaId = dto.CapaId ?? string.Empty,
            OrganizationId = organizationId.Value
        };

        var created = await _auditRepository.CreateFindingAsync(finding);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Finding>.SuccessResponse(created, "Finding created successfully."));
    }

    /// <summary>
    /// Update existing Finding
    /// </summary>
    [HttpPut("{id}")]
    [Audit(Module = "Audits", Entity = "Finding", Action = AuditActionType.Update)]
    [ProducesResponseType(typeof(ApiResponse<Finding>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Finding>), 404)]
    public async Task<ActionResult<ApiResponse<Finding>>> Update(string id, [FromBody] FindingRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<Finding>.FailureResponse("A specific organization context is required to update this record."));

        var existing = await _auditRepository.GetFindingByIdAsync(id, organizationId);
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

        var updated = await _auditRepository.UpdateFindingAsync(existing, organizationId.Value);
        return Ok(ApiResponse<Finding>.SuccessResponse(updated!, "Finding updated successfully."));
    }

    /// <summary>
    /// Delete a Finding
    /// </summary>
    [HttpDelete("{id}")]
    [Audit(Module = "Audits", Entity = "Finding", Action = AuditActionType.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<bool>.FailureResponse("A specific organization context is required to delete this record."));

        var deleted = await _auditRepository.DeleteFindingAsync(id, organizationId.Value);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Finding with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Finding deleted successfully."));
    }
}
