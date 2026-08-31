using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// Compliance Tasks and Task Templates Management API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;

    public TasksController(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    /// <summary>
    /// Get paginated and filtered list of Compliance Tasks
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TaskItem>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<TaskItem>>>> GetPaged([FromQuery] PagedRequestDto request)
    {
        var paged = await _taskRepository.GetPagedTasksAsync(request);
        return Ok(ApiResponse<PagedResponse<TaskItem>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all Compliance Tasks
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<TaskItem>>), 200)]
    public async Task<ActionResult<ApiResponse<List<TaskItem>>>> GetAll()
    {
        var tasks = await _taskRepository.GetAllTasksAsync();
        return Ok(ApiResponse<List<TaskItem>>.SuccessResponse(tasks));
    }

    /// <summary>
    /// Get Compliance Task details by Identifier
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TaskItem>), 200)]
    [ProducesResponseType(typeof(ApiResponse<TaskItem>), 404)]
    public async Task<ActionResult<ApiResponse<TaskItem>>> GetById(string id)
    {
        var task = await _taskRepository.GetTaskByIdAsync(id);
        if (task == null)
            return NotFound(ApiResponse<TaskItem>.FailureResponse($"Task with ID '{id}' was not found."));

        return Ok(ApiResponse<TaskItem>.SuccessResponse(task));
    }

    /// <summary>
    /// Get recurring Task Templates
    /// </summary>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(ApiResponse<List<TaskTemplate>>), 200)]
    public async Task<ActionResult<ApiResponse<List<TaskTemplate>>>> GetTemplates()
    {
        var templates = await _taskRepository.GetTaskTemplatesAsync();
        return Ok(ApiResponse<List<TaskTemplate>>.SuccessResponse(templates));
    }

    /// <summary>
    /// Create a new Compliance Task
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskItem>), 201)]
    public async Task<ActionResult<ApiResponse<TaskItem>>> Create([FromBody] TaskItemRequestDto dto)
    {
        var task = new TaskItem
        {
            Code = dto.Code,
            Title = dto.Title,
            ControlId = dto.ControlId ?? string.Empty,
            RiskId = dto.RiskId ?? string.Empty,
            Owner = dto.Owner,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            Status = dto.Status,
            EvidenceId = dto.EvidenceId ?? string.Empty,
            Comments = dto.Comments ?? string.Empty
        };

        var created = await _taskRepository.CreateTaskAsync(task);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<TaskItem>.SuccessResponse(created, "Task created successfully."));
    }

    /// <summary>
    /// Update existing Compliance Task
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TaskItem>), 200)]
    [ProducesResponseType(typeof(ApiResponse<TaskItem>), 404)]
    public async Task<ActionResult<ApiResponse<TaskItem>>> Update(string id, [FromBody] TaskItemRequestDto dto)
    {
        var existing = await _taskRepository.GetTaskByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse<TaskItem>.FailureResponse($"Task with ID '{id}' was not found."));

        existing.Title = dto.Title;
        existing.ControlId = dto.ControlId ?? string.Empty;
        existing.RiskId = dto.RiskId ?? string.Empty;
        existing.Owner = dto.Owner;
        existing.Priority = dto.Priority;
        existing.DueDate = dto.DueDate;
        existing.Status = dto.Status;
        existing.EvidenceId = dto.EvidenceId ?? string.Empty;
        existing.Comments = dto.Comments ?? string.Empty;

        var updated = await _taskRepository.UpdateTaskAsync(existing);
        return Ok(ApiResponse<TaskItem>.SuccessResponse(updated!, "Task updated successfully."));
    }

    /// <summary>
    /// Update task workflow status
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(string id, [FromBody] TaskStatusUpdateDto dto)
    {
        var updated = await _taskRepository.UpdateTaskStatusAsync(id, dto.Status);
        if (!updated)
            return NotFound(ApiResponse<bool>.FailureResponse($"Task with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Task status updated successfully."));
    }

    /// <summary>
    /// Delete a Compliance Task
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var deleted = await _taskRepository.DeleteTaskAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Task with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Task deleted successfully."));
    }
}
