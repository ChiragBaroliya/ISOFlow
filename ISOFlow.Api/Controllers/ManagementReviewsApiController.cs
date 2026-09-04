using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
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
        var paged = await _reviewRepository.GetPagedReviewsAsync(request);
        return Ok(ApiResponse<PagedResponse<ManagementReview>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all Management Reviews
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<ManagementReview>>), 200)]
    public async Task<ActionResult<ApiResponse<List<ManagementReview>>>> GetAll()
    {
        var reviews = await _reviewRepository.GetAllReviewsAsync();
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
        var review = await _reviewRepository.GetReviewByIdAsync(id);
        if (review == null)
            return NotFound(ApiResponse<ManagementReview>.FailureResponse($"Review with ID '{id}' was not found."));

        return Ok(ApiResponse<ManagementReview>.SuccessResponse(review));
    }

    /// <summary>
    /// Log a new Management Review session
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ManagementReview>), 201)]
    public async Task<ActionResult<ApiResponse<ManagementReview>>> Create([FromBody] ManagementReviewRequestDto dto)
    {
        var review = new ManagementReview
        {
            Code = dto.Code,
            Title = dto.Title,
            Period = dto.Period,
            ReviewDate = dto.ReviewDate,
            ChairPerson = dto.ChairPerson,
            Attendees = dto.Attendees ?? new List<string>(),
            Summary = dto.Summary,
            Status = dto.Status
        };

        var created = await _reviewRepository.CreateReviewAsync(review);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ManagementReview>.SuccessResponse(created, "Management review created successfully."));
    }

    /// <summary>
    /// Update existing Management Review
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ManagementReview>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ManagementReview>), 404)]
    public async Task<ActionResult<ApiResponse<ManagementReview>>> Update(string id, [FromBody] ManagementReviewRequestDto dto)
    {
        var existing = await _reviewRepository.GetReviewByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse<ManagementReview>.FailureResponse($"Review with ID '{id}' was not found."));

        existing.Title = dto.Title;
        existing.Period = dto.Period;
        existing.ReviewDate = dto.ReviewDate;
        existing.ChairPerson = dto.ChairPerson;
        existing.Attendees = dto.Attendees ?? new List<string>();
        existing.Summary = dto.Summary;
        existing.Status = dto.Status;

        var updated = await _reviewRepository.UpdateReviewAsync(existing);
        return Ok(ApiResponse<ManagementReview>.SuccessResponse(updated!, "Management review updated successfully."));
    }

    /// <summary>
    /// Delete a Management Review
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var deleted = await _reviewRepository.DeleteReviewAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Review with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Management review deleted successfully."));
    }
}
