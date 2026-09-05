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
/// Compliance Tasks and Task Templates Management API
/// </summary>
[Authorize]
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
        var organizationId = User.GetOrganizationIdOrNull();
        var paged = await _taskRepository.GetPagedTasksAsync(request, organizationId);
        return Ok(ApiResponse<PagedResponse<TaskItem>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all Compliance Tasks
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<TaskItem>>), 200)]
    public async Task<ActionResult<ApiResponse<List<TaskItem>>>> GetAll()
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var tasks = await _taskRepository.GetAllTasksAsync(organizationId);
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
        var organizationId = User.GetOrganizationIdOrNull();
        var task = await _taskRepository.GetTaskByIdAsync(id, organizationId);
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
        var organizationId = User.GetOrganizationIdOrNull();
        var templates = await _taskRepository.GetTaskTemplatesAsync(organizationId);
        return Ok(ApiResponse<List<TaskTemplate>>.SuccessResponse(templates));
    }

    /// <summary>
    /// Create a new recurring Task Template
    /// </summary>
    [HttpPost("templates")]
    [Audit(Module = "Tasks", Entity = "TaskTemplate", Action = AuditActionType.Create)]
    [ProducesResponseType(typeof(ApiResponse<TaskTemplate>), 201)]
    public async Task<ActionResult<ApiResponse<TaskTemplate>>> CreateTemplate([FromBody] TaskTemplateRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<TaskTemplate>.FailureResponse("A specific organization context is required to create this record."));

        var template = new TaskTemplate
        {
            Code = dto.Code,
            Title = dto.Title,
            Description = dto.Description ?? string.Empty,
            Frequency = dto.Frequency,
            DefaultOwner = dto.DefaultOwner,
            RelatedControlId = dto.RelatedControlId ?? string.Empty,
            OrganizationId = organizationId.Value
        };

        var created = await _taskRepository.CreateTaskTemplateAsync(template);
        return CreatedAtAction(nameof(GetTemplates), ApiResponse<TaskTemplate>.SuccessResponse(created, "Task template created successfully."));
    }

    /// <summary>
    /// Update an existing recurring Task Template
    /// </summary>
    [HttpPut("templates/{id}")]
    [Audit(Module = "Tasks", Entity = "TaskTemplate", Action = AuditActionType.Update)]
    [ProducesResponseType(typeof(ApiResponse<TaskTemplate>), 200)]
    [ProducesResponseType(typeof(ApiResponse<TaskTemplate>), 404)]
    public async Task<ActionResult<ApiResponse<TaskTemplate>>> UpdateTemplate(string id, [FromBody] TaskTemplateRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<TaskTemplate>.FailureResponse("A specific organization context is required to update this record."));

        var existing = await _taskRepository.GetTaskTemplateByIdAsync(id, organizationId);
        if (existing == null)
            return NotFound(ApiResponse<TaskTemplate>.FailureResponse($"Task template with ID '{id}' was not found."));

        existing.Title = dto.Title;
        existing.Description = dto.Description ?? string.Empty;
        existing.Frequency = dto.Frequency;
        existing.DefaultOwner = dto.DefaultOwner;
        existing.RelatedControlId = dto.RelatedControlId ?? string.Empty;

        var updated = await _taskRepository.UpdateTaskTemplateAsync(existing, organizationId.Value);
        return Ok(ApiResponse<TaskTemplate>.SuccessResponse(updated!, "Task template updated successfully."));
    }

    /// <summary>
    /// Delete a recurring Task Template
    /// </summary>
    [HttpDelete("templates/{id}")]
    [Audit(Module = "Tasks", Entity = "TaskTemplate", Action = AuditActionType.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTemplate(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<bool>.FailureResponse("A specific organization context is required to delete this record."));

        var deleted = await _taskRepository.DeleteTaskTemplateAsync(id, organizationId.Value);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Task template with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Task template deleted successfully."));
    }

    /// <summary>
    /// Generate a new Compliance Task instance from a recurring Task Template. The new task's due
    /// date is computed from the template's frequency, counted forward from today.
    /// </summary>
    [HttpPost("templates/{id}/generate")]
    [Audit(Module = "Tasks", Entity = "Task", Action = AuditActionType.Create)]
    [ProducesResponseType(typeof(ApiResponse<TaskItem>), 201)]
    [ProducesResponseType(typeof(ApiResponse<TaskItem>), 404)]
    public async Task<ActionResult<ApiResponse<TaskItem>>> GenerateFromTemplate(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<TaskItem>.FailureResponse("A specific organization context is required to create this record."));

        var template = await _taskRepository.GetTaskTemplateByIdAsync(id, organizationId);
        if (template == null)
            return NotFound(ApiResponse<TaskItem>.FailureResponse($"Task template with ID '{id}' was not found."));

        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            Code = $"{template.Code}-{now:yyyyMMdd}",
            Title = template.Title,
            ControlId = template.RelatedControlId,
            Owner = template.DefaultOwner,
            Priority = TaskPriority.Medium,
            DueDate = template.Frequency.GetNextDueDate(now),
            Status = ComplianceTaskStatus.NotStarted,
            Comments = template.Description,
            OrganizationId = organizationId.Value
        };

        var created = await _taskRepository.CreateTaskAsync(task);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<TaskItem>.SuccessResponse(created, $"Task generated from template '{template.Code}'."));
    }

    /// <summary>
    /// Create a new Compliance Task
    /// </summary>
    [HttpPost]
    [Audit(Module = "Tasks", Entity = "Task", Action = AuditActionType.Create)]
    [ProducesResponseType(typeof(ApiResponse<TaskItem>), 201)]
    public async Task<ActionResult<ApiResponse<TaskItem>>> Create([FromBody] TaskItemRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<TaskItem>.FailureResponse("A specific organization context is required to create this record."));

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
            Comments = dto.Comments ?? string.Empty,
            OrganizationId = organizationId.Value
        };

        var created = await _taskRepository.CreateTaskAsync(task);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<TaskItem>.SuccessResponse(created, "Task created successfully."));
    }

    /// <summary>
    /// Update existing Compliance Task
    /// </summary>
    [HttpPut("{id}")]
    [Audit(Module = "Tasks", Entity = "Task", Action = AuditActionType.Update)]
    [ProducesResponseType(typeof(ApiResponse<TaskItem>), 200)]
    [ProducesResponseType(typeof(ApiResponse<TaskItem>), 404)]
    public async Task<ActionResult<ApiResponse<TaskItem>>> Update(string id, [FromBody] TaskItemRequestDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<TaskItem>.FailureResponse("A specific organization context is required to update this record."));

        var existing = await _taskRepository.GetTaskByIdAsync(id, organizationId);
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

        var updated = await _taskRepository.UpdateTaskAsync(existing, organizationId.Value);
        return Ok(ApiResponse<TaskItem>.SuccessResponse(updated!, "Task updated successfully."));
    }

    /// <summary>
    /// Update task workflow status
    /// </summary>
    [HttpPatch("{id}/status")]
    [Audit(Module = "Tasks", Entity = "Task", Action = AuditActionType.StatusChange)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(string id, [FromBody] TaskStatusUpdateDto dto)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<bool>.FailureResponse("A specific organization context is required to update this record."));

        var updated = await _taskRepository.UpdateTaskStatusAsync(id, dto.Status, organizationId.Value);
        if (!updated)
            return NotFound(ApiResponse<bool>.FailureResponse($"Task with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Task status updated successfully."));
    }

    /// <summary>
    /// Delete a Compliance Task
    /// </summary>
    [HttpDelete("{id}")]
    [Audit(Module = "Tasks", Entity = "Task", Action = AuditActionType.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        if (organizationId == null)
            return BadRequest(ApiResponse<bool>.FailureResponse("A specific organization context is required to delete this record."));

        var deleted = await _taskRepository.DeleteTaskAsync(id, organizationId.Value);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"Task with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Task deleted successfully."));
    }
}
