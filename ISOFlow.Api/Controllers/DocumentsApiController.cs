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
/// Compliance Documents (Policies and Business Processes) Management API
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentsController(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    // ── Policies ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Get paginated and filtered list of Policies
    /// </summary>
    [HttpGet("policies")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<Policy>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<Policy>>>> GetPagedPolicies([FromQuery] PagedRequestDto request)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var paged = await _documentRepository.GetPagedPoliciesAsync(request, organizationId);
        return Ok(ApiResponse<PagedResponse<Policy>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get Policy details by Identifier
    /// </summary>
    [HttpGet("policies/{id}")]
    [ProducesResponseType(typeof(ApiResponse<Policy>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Policy>), 404)]
    public async Task<ActionResult<ApiResponse<Policy>>> GetPolicyById(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var policy = await _documentRepository.GetPolicyByIdAsync(id, organizationId);
        if (policy == null)
            return NotFound(ApiResponse<Policy>.FailureResponse($"Policy with ID '{id}' was not found."));

        return Ok(ApiResponse<Policy>.SuccessResponse(policy));
    }

    /// <summary>
    /// Create a new Policy
    /// </summary>
    [HttpPost("policies")]
    [Audit(Module = "Documents", Entity = "Policy", Action = AuditActionType.Create)]
    [ProducesResponseType(typeof(ApiResponse<Policy>), 201)]
    public async Task<ActionResult<ApiResponse<Policy>>> CreatePolicy([FromBody] PolicyRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<Policy>.FailureResponse("A specific organization context is required to create this record."));

        var policy = new Policy
        {
            Code = dto.Code,
            Title = dto.Title,
            Version = dto.Version,
            Owner = dto.Owner,
            EffectiveDate = dto.EffectiveDate,
            NextReviewDate = dto.NextReviewDate,
            Status = dto.Status,
            FilePath = dto.FilePath,
            LinkedControlIds = dto.LinkedControlIds ?? new List<string>(),
            LinkedProcessIds = dto.LinkedProcessIds ?? new List<string>(),
            OrganizationId = organizationId.Value
        };

        var created = await _documentRepository.CreatePolicyAsync(policy);
        return CreatedAtAction(nameof(GetPolicyById), new { id = created.Id }, ApiResponse<Policy>.SuccessResponse(created, "Policy created successfully."));
    }

    /// <summary>
    /// Update existing Policy
    /// </summary>
    [HttpPut("policies/{id}")]
    [Audit(Module = "Documents", Entity = "Policy", Action = AuditActionType.Update)]
    [ProducesResponseType(typeof(ApiResponse<Policy>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Policy>), 404)]
    public async Task<ActionResult<ApiResponse<Policy>>> UpdatePolicy(string id, [FromBody] PolicyRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<Policy>.FailureResponse("A specific organization context is required to update this record."));

        var existing = await _documentRepository.GetPolicyByIdAsync(id, organizationId);
        if (existing == null)
            return NotFound(ApiResponse<Policy>.FailureResponse($"Policy with ID '{id}' was not found."));

        existing.Title = dto.Title;
        existing.Version = dto.Version;
        existing.Owner = dto.Owner;
        existing.EffectiveDate = dto.EffectiveDate;
        existing.NextReviewDate = dto.NextReviewDate;
        existing.Status = dto.Status;
        existing.FilePath = dto.FilePath;
        existing.LinkedControlIds = dto.LinkedControlIds ?? new List<string>();
        existing.LinkedProcessIds = dto.LinkedProcessIds ?? new List<string>();

        var updated = await _documentRepository.UpdatePolicyAsync(existing, organizationId.Value);
        return Ok(ApiResponse<Policy>.SuccessResponse(updated!, "Policy updated successfully."));
    }

    /// <summary>
    /// Delete a Policy
    /// </summary>
    [HttpDelete("policies/{id}")]
    [Audit(Module = "Documents", Entity = "Policy", Action = AuditActionType.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePolicy(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<bool>.FailureResponse("A specific organization context is required to delete this record."));

        var deleted = await _documentRepository.DeletePolicyAsync(id, organizationId.Value);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Policy with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Policy deleted successfully."));
    }

    // ── Processes ────────────────────────────────────────────────────────────

    /// <summary>
    /// Get paginated and filtered list of Business Processes
    /// </summary>
    [HttpGet("processes")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<Process>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<Process>>>> GetPagedProcesses([FromQuery] PagedRequestDto request)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var paged = await _documentRepository.GetPagedProcessesAsync(request, organizationId);
        return Ok(ApiResponse<PagedResponse<Process>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get Business Process details by Identifier
    /// </summary>
    [HttpGet("processes/{id}")]
    [ProducesResponseType(typeof(ApiResponse<Process>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Process>), 404)]
    public async Task<ActionResult<ApiResponse<Process>>> GetProcessById(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var process = await _documentRepository.GetProcessByIdAsync(id, organizationId);
        if (process == null)
            return NotFound(ApiResponse<Process>.FailureResponse($"Process with ID '{id}' was not found."));

        return Ok(ApiResponse<Process>.SuccessResponse(process));
    }

    /// <summary>
    /// Create a new Business Process with step workflow
    /// </summary>
    [HttpPost("processes")]
    [Audit(Module = "Documents", Entity = "Process", Action = AuditActionType.Create)]
    [ProducesResponseType(typeof(ApiResponse<Process>), 201)]
    public async Task<ActionResult<ApiResponse<Process>>> CreateProcess([FromBody] ProcessRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<Process>.FailureResponse("A specific organization context is required to create this record."));

        var process = new Process
        {
            Code = dto.Code,
            Title = dto.Title,
            Category = dto.Category,
            Owner = dto.Owner,
            Description = dto.Description,
            Version = dto.Version,
            Status = dto.Status,
            Steps = dto.Steps ?? new List<string>(),
            PolicyId = dto.PolicyId,
            ControlIds = dto.ControlIds ?? new List<string>(),
            OrganizationId = organizationId.Value
        };

        var created = await _documentRepository.CreateProcessAsync(process);
        return CreatedAtAction(nameof(GetProcessById), new { id = created.Id }, ApiResponse<Process>.SuccessResponse(created, "Process created successfully."));
    }

    /// <summary>
    /// Update existing Business Process
    /// </summary>
    [HttpPut("processes/{id}")]
    [Audit(Module = "Documents", Entity = "Process", Action = AuditActionType.Update)]
    [ProducesResponseType(typeof(ApiResponse<Process>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Process>), 404)]
    public async Task<ActionResult<ApiResponse<Process>>> UpdateProcess(string id, [FromBody] ProcessRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<Process>.FailureResponse("A specific organization context is required to update this record."));

        var existing = await _documentRepository.GetProcessByIdAsync(id, organizationId);
        if (existing == null)
            return NotFound(ApiResponse<Process>.FailureResponse($"Process with ID '{id}' was not found."));

        existing.Title = dto.Title;
        existing.Category = dto.Category;
        existing.Owner = dto.Owner;
        existing.Description = dto.Description;
        existing.Version = dto.Version;
        existing.Status = dto.Status;
        existing.Steps = dto.Steps ?? new List<string>();
        existing.PolicyId = dto.PolicyId;
        existing.ControlIds = dto.ControlIds ?? new List<string>();

        var updated = await _documentRepository.UpdateProcessAsync(existing, organizationId.Value);
        return Ok(ApiResponse<Process>.SuccessResponse(updated!, "Process updated successfully."));
    }

    /// <summary>
    /// Archive a Business Process
    /// </summary>
    [HttpPost("processes/{id}/archive")]
    [Audit(Module = "Documents", Entity = "Process", Action = AuditActionType.Deactivate)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> ArchiveProcess(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<bool>.FailureResponse("A specific organization context is required to archive this record."));

        var archived = await _documentRepository.ArchiveProcessAsync(id, organizationId.Value);
        if (!archived)
            return NotFound(ApiResponse<bool>.FailureResponse($"Process with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Process archived successfully."));
    }
}
