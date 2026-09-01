using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// Information Security and Compliance Risk Management API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class RisksController : ControllerBase
{
    private readonly IRiskRepository _riskRepository;

    public RisksController(IRiskRepository riskRepository)
    {
        _riskRepository = riskRepository;
    }

    /// <summary>
    /// Get paginated and filtered list of Risks
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<Risk>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<Risk>>>> GetPaged([FromQuery] PagedRequestDto request)
    {
        var paged = await _riskRepository.GetPagedRisksAsync(request);
        return Ok(ApiResponse<PagedResponse<Risk>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all registered Risks
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<Risk>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Risk>>>> GetAll()
    {
        var risks = await _riskRepository.GetAllRisksAsync();
        return Ok(ApiResponse<List<Risk>>.SuccessResponse(risks));
    }

    /// <summary>
    /// Get Risk details by Identifier
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Risk>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Risk>), 404)]
    public async Task<ActionResult<ApiResponse<Risk>>> GetById(string id)
    {
        var risk = await _riskRepository.GetRiskByIdAsync(id);
        if (risk == null)
            return NotFound(ApiResponse<Risk>.FailureResponse($"Risk with ID '{id}' was not found."));

        return Ok(ApiResponse<Risk>.SuccessResponse(risk));
    }

    /// <summary>
    /// Register a new Risk Assessment (with optional Treatment Plan)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Risk>), 201)]
    public async Task<ActionResult<ApiResponse<Risk>>> Create([FromBody] RiskRequestDto dto)
    {
        var risk = new Risk
        {
            Code = dto.Code,
            Title = dto.Title,
            Description = dto.Description,
            Asset = dto.Asset,
            Department = dto.Department,
            Owner = dto.Owner,
            Likelihood = dto.Likelihood,
            Impact = dto.Impact,
            ControlId = dto.ControlId ?? string.Empty,
            Status = dto.Status
        };

        RiskTreatment? treatment = null;
        if (dto.Treatment != null)
        {
            treatment = new RiskTreatment
            {
                Option = dto.Treatment.Option,
                TreatmentPlan = dto.Treatment.TreatmentPlan,
                Owner = dto.Treatment.Owner,
                TargetDate = dto.Treatment.TargetDate,
                ResidualLikelihood = dto.Treatment.ResidualLikelihood,
                ResidualImpact = dto.Treatment.ResidualImpact,
                Status = dto.Treatment.Status
            };
        }

        var created = await _riskRepository.CreateRiskAsync(risk, treatment);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Risk>.SuccessResponse(created, "Risk created successfully."));
    }

    /// <summary>
    /// Update existing Risk details and Treatment Plan
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Risk>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Risk>), 404)]
    public async Task<ActionResult<ApiResponse<Risk>>> Update(string id, [FromBody] RiskRequestDto dto)
    {
        var existing = await _riskRepository.GetRiskByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse<Risk>.FailureResponse($"Risk with ID '{id}' was not found."));

        existing.Code = dto.Code;
        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.Asset = dto.Asset;
        existing.Department = dto.Department;
        existing.Owner = dto.Owner;
        existing.Likelihood = dto.Likelihood;
        existing.Impact = dto.Impact;
        existing.ControlId = dto.ControlId ?? string.Empty;
        existing.Status = dto.Status;

        RiskTreatment? treatment = null;
        if (dto.Treatment != null)
        {
            treatment = new RiskTreatment
            {
                RiskId = existing.Id,
                Option = dto.Treatment.Option,
                TreatmentPlan = dto.Treatment.TreatmentPlan,
                Owner = dto.Treatment.Owner,
                TargetDate = dto.Treatment.TargetDate,
                ResidualLikelihood = dto.Treatment.ResidualLikelihood,
                ResidualImpact = dto.Treatment.ResidualImpact,
                Status = dto.Treatment.Status
            };
        }

        var updated = await _riskRepository.UpdateRiskAsync(existing, treatment);
        return Ok(ApiResponse<Risk>.SuccessResponse(updated!, "Risk updated successfully."));
    }

    /// <summary>
    /// Delete a Risk Assessment
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var deleted = await _riskRepository.DeleteRiskAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Risk with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Risk deleted successfully."));
    }

    /// <summary>
    /// Get Treatment Plan for a specific Risk
    /// </summary>
    [HttpGet("{id}/treatment")]
    [ProducesResponseType(typeof(ApiResponse<RiskTreatment>), 200)]
    [ProducesResponseType(typeof(ApiResponse<RiskTreatment>), 404)]
    public async Task<ActionResult<ApiResponse<RiskTreatment>>> GetTreatment(string id)
    {
        var treatment = await _riskRepository.GetRiskTreatmentByRiskIdAsync(id);
        if (treatment == null)
            return NotFound(ApiResponse<RiskTreatment>.FailureResponse($"Treatment for Risk ID '{id}' was not found."));

        return Ok(ApiResponse<RiskTreatment>.SuccessResponse(treatment));
    }

    /// <summary>
    /// Get paginated list of all Risk Treatments
    /// </summary>
    [HttpGet("treatments")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<RiskTreatment>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<RiskTreatment>>>> GetPagedTreatments([FromQuery] PagedRequestDto request)
    {
        var paged = await _riskRepository.GetPagedRiskTreatmentsAsync(request);
        return Ok(ApiResponse<PagedResponse<RiskTreatment>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get 5x5 Risk Heat Map Matrix aggregations
    /// </summary>
    [HttpGet("matrix")]
    [ProducesResponseType(typeof(ApiResponse<List<RiskMatrixCellDto>>), 200)]
    public async Task<ActionResult<ApiResponse<List<RiskMatrixCellDto>>>> GetRiskMatrix()
    {
        var matrix = await _riskRepository.GetRiskMatrixDataAsync();
        return Ok(ApiResponse<List<RiskMatrixCellDto>>.SuccessResponse(matrix));
    }
}
