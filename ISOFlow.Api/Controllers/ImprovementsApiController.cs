using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// Continual Improvement Initiatives API
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ImprovementsController : ControllerBase
{
    private readonly IManagementReviewRepository _reviewRepository;

    public ImprovementsController(IManagementReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    /// <summary>
    /// Get paginated and filtered list of Continual Improvements
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<Improvement>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<Improvement>>>> GetPaged([FromQuery] PagedRequestDto request)
    {
        var paged = await _reviewRepository.GetPagedImprovementsAsync(request);
        return Ok(ApiResponse<PagedResponse<Improvement>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all Continual Improvements
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<Improvement>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Improvement>>>> GetAll()
    {
        var list = await _reviewRepository.GetAllImprovementsAsync();
        return Ok(ApiResponse<List<Improvement>>.SuccessResponse(list));
    }

    /// <summary>
    /// Get Continual Improvement details by Identifier
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Improvement>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Improvement>), 404)]
    public async Task<ActionResult<ApiResponse<Improvement>>> GetById(string id)
    {
        var item = await _reviewRepository.GetImprovementByIdAsync(id);
        if (item == null)
            return NotFound(ApiResponse<Improvement>.FailureResponse($"Improvement with ID '{id}' was not found."));

        return Ok(ApiResponse<Improvement>.SuccessResponse(item));
    }

    /// <summary>
    /// Create a new Continual Improvement initiative
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Improvement>), 201)]
    public async Task<ActionResult<ApiResponse<Improvement>>> Create([FromBody] ImprovementRequestDto dto)
    {
        var item = new Improvement
        {
            Code = dto.Code,
            Title = dto.Title,
            CurrentState = dto.CurrentState,
            FutureState = dto.FutureState,
            Source = dto.Source,
            ExpectedBenefit = dto.ExpectedBenefit,
            Owner = dto.Owner,
            Status = dto.Status,
            RelatedReviewId = dto.RelatedReviewId ?? string.Empty,
            RelatedFindingId = dto.RelatedFindingId ?? string.Empty
        };

        var created = await _reviewRepository.CreateImprovementAsync(item);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Improvement>.SuccessResponse(created, "Improvement created successfully."));
    }

    /// <summary>
    /// Update existing Continual Improvement
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Improvement>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Improvement>), 404)]
    public async Task<ActionResult<ApiResponse<Improvement>>> Update(string id, [FromBody] ImprovementRequestDto dto)
    {
        var existing = await _reviewRepository.GetImprovementByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse<Improvement>.FailureResponse($"Improvement with ID '{id}' was not found."));

        existing.Title = dto.Title;
        existing.CurrentState = dto.CurrentState;
        existing.FutureState = dto.FutureState;
        existing.Source = dto.Source;
        existing.ExpectedBenefit = dto.ExpectedBenefit;
        existing.Owner = dto.Owner;
        existing.Status = dto.Status;
        existing.RelatedReviewId = dto.RelatedReviewId ?? string.Empty;
        existing.RelatedFindingId = dto.RelatedFindingId ?? string.Empty;

        var updated = await _reviewRepository.UpdateImprovementAsync(existing);
        return Ok(ApiResponse<Improvement>.SuccessResponse(updated!, "Improvement updated successfully."));
    }

    /// <summary>
    /// Delete a Continual Improvement initiative
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var deleted = await _reviewRepository.DeleteImprovementAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Improvement with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Improvement deleted successfully."));
    }
}
