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
/// Audit Evidence and Artifacts Vault API
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EvidenceController : ControllerBase
{
    private readonly IEvidenceRepository _evidenceRepository;

    public EvidenceController(IEvidenceRepository evidenceRepository)
    {
        _evidenceRepository = evidenceRepository;
    }

    /// <summary>
    /// Get paginated and filtered list of Evidence records
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<Evidence>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<Evidence>>>> GetPaged([FromQuery] PagedRequestDto request)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var paged = await _evidenceRepository.GetPagedEvidenceAsync(request, organizationId);
        return Ok(ApiResponse<PagedResponse<Evidence>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all Evidence records
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<Evidence>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Evidence>>>> GetAll()
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var evidence = await _evidenceRepository.GetAllEvidenceAsync(organizationId);
        return Ok(ApiResponse<List<Evidence>>.SuccessResponse(evidence));
    }

    /// <summary>
    /// Get Evidence details by Identifier
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Evidence>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Evidence>), 404)]
    public async Task<ActionResult<ApiResponse<Evidence>>> GetById(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var evidence = await _evidenceRepository.GetEvidenceByIdAsync(id, organizationId);
        if (evidence == null)
            return NotFound(ApiResponse<Evidence>.FailureResponse($"Evidence with ID '{id}' was not found."));

        return Ok(ApiResponse<Evidence>.SuccessResponse(evidence));
    }

    /// <summary>
    /// Upload or register a new Evidence record
    /// </summary>
    [HttpPost]
    [Audit(Module = "Evidence", Entity = "Evidence", Action = AuditActionType.Create)]
    [ProducesResponseType(typeof(ApiResponse<Evidence>), 201)]
    public async Task<ActionResult<ApiResponse<Evidence>>> Create([FromBody] EvidenceRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<Evidence>.FailureResponse("A specific organization context is required to create this record."));

        var entity = new Evidence
        {
            Code = dto.Code,
            Name = dto.Name,
            Type = dto.Type,
            ControlId = dto.ControlId ?? string.Empty,
            RequirementId = dto.RequirementId ?? string.Empty,
            TaskId = dto.TaskId ?? string.Empty,
            AuditId = dto.AuditId ?? string.Empty,
            UploadedBy = dto.UploadedBy,
            UploadDate = dto.UploadDate,
            ExpiryDate = dto.ExpiryDate,
            Status = dto.Status,
            FileUrl = dto.FileUrl,
            OrganizationId = organizationId.Value
        };

        var created = await _evidenceRepository.CreateEvidenceAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Evidence>.SuccessResponse(created, "Evidence registered successfully."));
    }

    /// <summary>
    /// Update existing Evidence metadata
    /// </summary>
    [HttpPut("{id}")]
    [Audit(Module = "Evidence", Entity = "Evidence", Action = AuditActionType.Update)]
    [ProducesResponseType(typeof(ApiResponse<Evidence>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Evidence>), 404)]
    public async Task<ActionResult<ApiResponse<Evidence>>> Update(string id, [FromBody] EvidenceRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<Evidence>.FailureResponse("A specific organization context is required to update this record."));

        var existing = await _evidenceRepository.GetEvidenceByIdAsync(id, organizationId);
        if (existing == null)
            return NotFound(ApiResponse<Evidence>.FailureResponse($"Evidence with ID '{id}' was not found."));

        existing.Name = dto.Name;
        existing.Type = dto.Type;
        existing.ControlId = dto.ControlId ?? string.Empty;
        existing.RequirementId = dto.RequirementId ?? string.Empty;
        existing.TaskId = dto.TaskId ?? string.Empty;
        existing.AuditId = dto.AuditId ?? string.Empty;
        existing.ExpiryDate = dto.ExpiryDate;
        existing.Status = dto.Status;
        existing.FileUrl = dto.FileUrl;

        var updated = await _evidenceRepository.UpdateEvidenceAsync(existing, organizationId.Value);
        return Ok(ApiResponse<Evidence>.SuccessResponse(updated!, "Evidence updated successfully."));
    }

    /// <summary>
    /// Delete an Evidence record
    /// </summary>
    [HttpDelete("{id}")]
    [Audit(Module = "Evidence", Entity = "Evidence", Action = AuditActionType.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<bool>.FailureResponse("A specific organization context is required to delete this record."));

        var deleted = await _evidenceRepository.DeleteEvidenceAsync(id, organizationId.Value);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Evidence with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Evidence deleted successfully."));
    }
}
