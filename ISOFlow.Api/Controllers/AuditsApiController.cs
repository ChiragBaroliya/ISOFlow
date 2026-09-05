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
/// Internal and External ISO Audits and Audit Programs API
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuditsController : ControllerBase
{
    private readonly IAuditRepository _auditRepository;

    public AuditsController(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    /// <summary>
    /// Get paginated list of Audit Programs
    /// </summary>
    [HttpGet("programs")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<AuditProgram>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<AuditProgram>>>> GetPagedPrograms([FromQuery] PagedRequestDto request)
    {
        var paged = await _auditRepository.GetPagedAuditProgramsAsync(request);
        return Ok(ApiResponse<PagedResponse<AuditProgram>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get paginated and filtered list of Audits
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<Audit>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<Audit>>>> GetPagedAudits([FromQuery] PagedRequestDto request)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var paged = await _auditRepository.GetPagedAuditsAsync(request, organizationId);
        return Ok(ApiResponse<PagedResponse<Audit>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all Audits
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<Audit>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Audit>>>> GetAllAudits()
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var audits = await _auditRepository.GetAllAuditsAsync(organizationId);
        return Ok(ApiResponse<List<Audit>>.SuccessResponse(audits));
    }

    /// <summary>
    /// Get Audit details by Identifier
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Audit>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Audit>), 404)]
    public async Task<ActionResult<ApiResponse<Audit>>> GetAuditById(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var audit = await _auditRepository.GetAuditByIdAsync(id, organizationId);
        if (audit == null)
            return NotFound(ApiResponse<Audit>.FailureResponse($"Audit with ID '{id}' was not found."));

        return Ok(ApiResponse<Audit>.SuccessResponse(audit));
    }

    /// <summary>
    /// Create a new Audit
    /// </summary>
    [HttpPost]
    [Audit(Module = "Audits", Entity = "Audit", Action = AuditActionType.Create)]
    [ProducesResponseType(typeof(ApiResponse<Audit>), 201)]
    public async Task<ActionResult<ApiResponse<Audit>>> CreateAudit([FromBody] AuditRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<Audit>.FailureResponse("A specific organization context is required to create this record."));

        var audit = new Audit
        {
            Code = dto.Code,
            Title = dto.Title,
            StandardId = dto.StandardId ?? string.Empty,
            LeadAuditor = dto.LeadAuditor,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = dto.Status,
            CompletionPercentage = dto.CompletionPercentage,
            Scope = dto.Scope,
            CheckListControlIds = dto.CheckListControlIds ?? new List<string>(),
            OrganizationId = organizationId.Value
        };

        var created = await _auditRepository.CreateAuditAsync(audit);
        return CreatedAtAction(nameof(GetAuditById), new { id = created.Id }, ApiResponse<Audit>.SuccessResponse(created, "Audit created successfully."));
    }

    /// <summary>
    /// Update existing Audit
    /// </summary>
    [HttpPut("{id}")]
    [Audit(Module = "Audits", Entity = "Audit", Action = AuditActionType.Update)]
    [ProducesResponseType(typeof(ApiResponse<Audit>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Audit>), 404)]
    public async Task<ActionResult<ApiResponse<Audit>>> UpdateAudit(string id, [FromBody] AuditRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<Audit>.FailureResponse("A specific organization context is required to update this record."));

        var existing = await _auditRepository.GetAuditByIdAsync(id, organizationId);
        if (existing == null)
            return NotFound(ApiResponse<Audit>.FailureResponse($"Audit with ID '{id}' was not found."));

        existing.Title = dto.Title;
        existing.StandardId = dto.StandardId ?? string.Empty;
        existing.LeadAuditor = dto.LeadAuditor;
        existing.StartDate = dto.StartDate;
        existing.EndDate = dto.EndDate;
        existing.Status = dto.Status;
        existing.CompletionPercentage = dto.CompletionPercentage;
        existing.Scope = dto.Scope;
        existing.CheckListControlIds = dto.CheckListControlIds ?? new List<string>();

        var updated = await _auditRepository.UpdateAuditAsync(existing, organizationId.Value);
        return Ok(ApiResponse<Audit>.SuccessResponse(updated!, "Audit updated successfully."));
    }

    /// <summary>
    /// Delete an Audit
    /// </summary>
    [HttpDelete("{id}")]
    [Audit(Module = "Audits", Entity = "Audit", Action = AuditActionType.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAudit(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<bool>.FailureResponse("A specific organization context is required to delete this record."));

        var deleted = await _auditRepository.DeleteAuditAsync(id, organizationId.Value);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Audit with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Audit deleted successfully."));
    }
}
