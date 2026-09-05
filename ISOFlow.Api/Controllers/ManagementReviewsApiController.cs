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
/// Executive Management Review and Governance API
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ManagementReviewsController : ControllerBase
{
    private readonly IManagementReviewRepository _reviewRepository;

    public ManagementReviewsController(IManagementReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    /// <summary>
    /// Get paginated and filtered list of Management Reviews
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ManagementReview>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<ManagementReview>>>> GetPaged([FromQuery] PagedRequestDto request)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var paged = await _reviewRepository.GetPagedReviewsAsync(request, organizationId);
        return Ok(ApiResponse<PagedResponse<ManagementReview>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all Management Reviews
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<ManagementReview>>), 200)]
    public async Task<ActionResult<ApiResponse<List<ManagementReview>>>> GetAll()
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var reviews = await _reviewRepository.GetAllReviewsAsync(organizationId);
        return Ok(ApiResponse<List<ManagementReview>>.SuccessResponse(reviews));
    }

    /// <summary>
    /// Get Management Review details by Identifier
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ManagementReview>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ManagementReview>), 404)]
    public async Task<ActionResult<ApiResponse<ManagementReview>>> GetById(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var review = await _reviewRepository.GetReviewByIdAsync(id, organizationId);
        if (review == null)
            return NotFound(ApiResponse<ManagementReview>.FailureResponse($"Review with ID '{id}' was not found."));

        return Ok(ApiResponse<ManagementReview>.SuccessResponse(review));
    }

    /// <summary>
    /// Log a new Management Review session
    /// </summary>
    [HttpPost]
    [Audit(Module = "Governance", Entity = "ManagementReview", Action = AuditActionType.Create)]
    [ProducesResponseType(typeof(ApiResponse<ManagementReview>), 201)]
    public async Task<ActionResult<ApiResponse<ManagementReview>>> Create([FromBody] ManagementReviewRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<ManagementReview>.FailureResponse("A specific organization context is required to create this record."));

        var review = new ManagementReview
        {
            Code = dto.Code,
            Title = dto.Title,
            Period = dto.Period,
            ReviewDate = dto.ReviewDate,
            ChairPerson = dto.ChairPerson,
            Attendees = dto.Attendees ?? new List<string>(),
            Summary = dto.Summary,
            Status = dto.Status,
            OrganizationId = organizationId.Value
        };

        var created = await _reviewRepository.CreateReviewAsync(review);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ManagementReview>.SuccessResponse(created, "Management review created successfully."));
    }

    /// <summary>
    /// Update existing Management Review
    /// </summary>
    [HttpPut("{id}")]
    [Audit(Module = "Governance", Entity = "ManagementReview", Action = AuditActionType.Update)]
    [ProducesResponseType(typeof(ApiResponse<ManagementReview>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ManagementReview>), 404)]
    public async Task<ActionResult<ApiResponse<ManagementReview>>> Update(string id, [FromBody] ManagementReviewRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<ManagementReview>.FailureResponse("A specific organization context is required to update this record."));

        var existing = await _reviewRepository.GetReviewByIdAsync(id, organizationId);
        if (existing == null)
            return NotFound(ApiResponse<ManagementReview>.FailureResponse($"Review with ID '{id}' was not found."));

        existing.Title = dto.Title;
        existing.Period = dto.Period;
        existing.ReviewDate = dto.ReviewDate;
        existing.ChairPerson = dto.ChairPerson;
        existing.Attendees = dto.Attendees ?? new List<string>();
        existing.Summary = dto.Summary;
        existing.Status = dto.Status;

        var updated = await _reviewRepository.UpdateReviewAsync(existing, organizationId.Value);
        return Ok(ApiResponse<ManagementReview>.SuccessResponse(updated!, "Management review updated successfully."));
    }

    /// <summary>
    /// Delete a Management Review
    /// </summary>
    [HttpDelete("{id}")]
    [Audit(Module = "Governance", Entity = "ManagementReview", Action = AuditActionType.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<bool>.FailureResponse("A specific organization context is required to delete this record."));

        var deleted = await _reviewRepository.DeleteReviewAsync(id, organizationId.Value);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Review with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Management review deleted successfully."));
    }
}
