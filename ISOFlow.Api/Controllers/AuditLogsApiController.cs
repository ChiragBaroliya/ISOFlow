using ISOFlow.Api.Extensions;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// Read-only access to the centralized, append-only Audit Log trail. There is deliberately no
/// create/update/delete action here — audit records are written exclusively by the automatic
/// AuditActionFilter pipeline (see ISOFlow.Api/Auditing), never through this API.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogsController(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    /// <summary>
    /// Get paginated, filtered Audit Logs (date range, module, entity, entity id, action, user, search).
    /// Restricted to SuperAdmin/Admin — this is the global cross-entity audit browser.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<AuditLogDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<ActionResult<ApiResponse<PagedResponse<AuditLogDto>>>> GetPaged([FromQuery] AuditLogFilterDto filter)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var paged = await _auditLogRepository.GetPagedAsync(filter, organizationId);
        return Ok(ApiResponse<PagedResponse<AuditLogDto>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get full detail for one audit entry, including the field-level Old Value / New Value diff.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDetailDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDetailDto>), 404)]
    public async Task<ActionResult<ApiResponse<AuditLogDetailDto>>> GetById(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var detail = await _auditLogRepository.GetByIdAsync(id, organizationId);
        if (detail == null)
            return NotFound(ApiResponse<AuditLogDetailDto>.FailureResponse($"Audit log entry with ID '{id}' was not found."));

        return Ok(ApiResponse<AuditLogDetailDto>.SuccessResponse(detail));
    }

    /// <summary>
    /// Full change history for one entity (e.g. one Risk, Control, Policy), newest first. Any
    /// authenticated user may view an entity's own history — this is not the privileged global
    /// browser above, just the "History" tab on a record the caller can already see.
    /// </summary>
    [HttpGet("history/{entityName}/{entityId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<AuditLogDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> GetHistory(string entityName, string entityId)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var history = await _auditLogRepository.GetHistoryAsync(entityName, entityId, organizationId);
        return Ok(ApiResponse<List<AuditLogDto>>.SuccessResponse(history));
    }
}
