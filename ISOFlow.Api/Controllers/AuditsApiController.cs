using ISOFlow.Api.Auditing;
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
        var paged = await _auditRepository.GetPagedAuditsAsync(request);
        return Ok(ApiResponse<PagedResponse<Audit>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all Audits
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<Audit>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Audit>>>> GetAllAudits()
    {
        var audits = await _auditRepository.GetAllAuditsAsync();
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
        var audit = await _auditRepository.GetAuditByIdAsync(id);
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
            CheckListControlIds = dto.CheckListControlIds ?? new List<string>()
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
        var existing = await _auditRepository.GetAuditByIdAsync(id);
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

        var updated = await _auditRepository.UpdateAuditAsync(existing);
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
        var deleted = await _auditRepository.DeleteAuditAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Audit with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Audit deleted successfully."));
    }
}
