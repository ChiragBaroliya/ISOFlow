using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// Tenant Organizations and Enterprise Scopes API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationRepository _orgRepository;

    public OrganizationsController(IOrganizationRepository orgRepository)
    {
        _orgRepository = orgRepository;
    }

    /// <summary>
    /// Get paginated and filtered list of Organizations
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<Organization>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<Organization>>>> GetPaged([FromQuery] PagedRequestDto request)
    {
        var paged = await _orgRepository.GetPagedOrganizationsAsync(request);
        return Ok(ApiResponse<PagedResponse<Organization>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all registered Organizations
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<Organization>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Organization>>>> GetAll()
    {
        var list = await _orgRepository.GetAllOrganizationsAsync();
        return Ok(ApiResponse<List<Organization>>.SuccessResponse(list));
    }

    /// <summary>
    /// Get Organization details by Identifier
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Organization>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Organization>), 404)]
    public async Task<ActionResult<ApiResponse<Organization>>> GetById(string id)
    {
        var org = await _orgRepository.GetOrganizationByIdAsync(id);
        if (org == null)
            return NotFound(ApiResponse<Organization>.FailureResponse($"Organization with ID '{id}' was not found."));

        return Ok(ApiResponse<Organization>.SuccessResponse(org));
    }

    /// <summary>
    /// Register a new Organization tenant (Requires SuperAdmin or Admin role)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<Organization>), 201)]
    public async Task<ActionResult<ApiResponse<Organization>>> Create([FromBody] OrganizationRequestDto dto)
    {
        var org = new Organization
        {
            Code = dto.Code,
            Name = dto.Name,
            Industry = dto.Industry,
            Employees = dto.Employees,
            Locations = dto.Locations ?? new List<string>(),
            PrimaryStandard = dto.PrimaryStandard,
            Status = dto.Status,
            CompliancePercentage = dto.CompliancePercentage,
            ContactEmail = dto.ContactEmail
        };

        var created = await _orgRepository.CreateOrganizationAsync(org);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Organization>.SuccessResponse(created, "Organization registered successfully."));
    }

    /// <summary>
    /// Update existing Organization (Requires SuperAdmin or Admin role)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<Organization>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Organization>), 404)]
    public async Task<ActionResult<ApiResponse<Organization>>> Update(string id, [FromBody] OrganizationRequestDto dto)
    {
        var existing = await _orgRepository.GetOrganizationByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse<Organization>.FailureResponse($"Organization with ID '{id}' was not found."));

        existing.Name = dto.Name;
        existing.Industry = dto.Industry;
        existing.Employees = dto.Employees;
        existing.Locations = dto.Locations ?? new List<string>();
        existing.PrimaryStandard = dto.PrimaryStandard;
        existing.Status = dto.Status;
        existing.CompliancePercentage = dto.CompliancePercentage;
        existing.ContactEmail = dto.ContactEmail;

        var updated = await _orgRepository.UpdateOrganizationAsync(existing);
        return Ok(ApiResponse<Organization>.SuccessResponse(updated!, "Organization updated successfully."));
    }

    /// <summary>
    /// Delete an Organization (Requires SuperAdmin or Admin role)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var deleted = await _orgRepository.DeleteOrganizationAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Organization with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Organization deleted successfully."));
    }
}
