using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class TaskRepository : BaseRepository, ITaskRepository
{
    public TaskRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<List<TaskItem>> GetAllTasksAsync()
    {
        return QueryMappedListAsync("SELECT * FROM sp_tasks_get_all()", r => new TaskItem
        {
            Id = r.id.ToString(),
            Code = (string)r.code,
            Title = (string)r.title,
            ControlId = r.control_id != null ? r.control_id.ToString() : string.Empty,
            RiskId = r.risk_id != null ? r.risk_id.ToString() : string.Empty,
            Owner = (string)r.owner,
            Priority = (TaskPriority)(int)r.priority,
            DueDate = (DateTime)r.due_date,
            Status = (ComplianceTaskStatus)(int)r.status,
            EvidenceId = r.evidence_id != null ? r.evidence_id.ToString() : string.Empty,
            CompletedDate = (DateTime?)r.completed_date,
            Comments = (string)r.comments ?? string.Empty
        });
    }

    public Task<PagedResponse<TaskItem>> GetPagedTasksAsync(PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", int.TryParse(request.StatusFilter, out var st) ? st : (int?)null);
        parameters.Add("p_owner", (string?)null);

        return QueryPagedAsync(
            "SELECT * FROM sp_tasks_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_status, @p_owner)",
            r => new TaskItem
            {
                Id = r.id.ToString(),
                Code = (string)r.code,
                Title = (string)r.title,
                ControlId = r.control_id != null ? r.control_id.ToString() : string.Empty,
                RiskId = r.risk_id != null ? r.risk_id.ToString() : string.Empty,
                Owner = (string)r.owner,
                Priority = (TaskPriority)(int)r.priority,
                DueDate = (DateTime)r.due_date,
                Status = (ComplianceTaskStatus)(int)r.status,
                EvidenceId = r.evidence_id != null ? r.evidence_id.ToString() : string.Empty,
                CompletedDate = (DateTime?)r.completed_date,
                Comments = (string)r.comments ?? string.Empty
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<TaskItem?> GetTaskByIdAsync(string id)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_tasks_get_by_id(@id)",
            r => new TaskItem
            {
                Id = r.id.ToString(),
                Code = (string)r.code,
                Title = (string)r.title,
                ControlId = r.control_id != null ? r.control_id.ToString() : string.Empty,
                RiskId = r.risk_id != null ? r.risk_id.ToString() : string.Empty,
                Owner = (string)r.owner,
                Priority = (TaskPriority)(int)r.priority,
                DueDate = (DateTime)r.due_date,
                Status = (ComplianceTaskStatus)(int)r.status,
                EvidenceId = r.evidence_id != null ? r.evidence_id.ToString() : string.Empty,
                CompletedDate = (DateTime?)r.completed_date,
                Comments = (string)r.comments ?? string.Empty
            },
            new { id });
    }

    public Task<List<TaskTemplate>> GetTaskTemplatesAsync()
    {
        return QueryMappedListAsync(
            "SELECT id, code, title, description, frequency, default_owner, related_control_id FROM task_templates ORDER BY id ASC",
            r => new TaskTemplate
            {
                Id = r.id.ToString(),
                Code = (string)r.code,
                Title = (string)r.title,
                Description = (string)r.description ?? string.Empty,
                Frequency = (string)r.frequency,
                DefaultOwner = (string)r.default_owner,
                RelatedControlId = r.related_control_id != null ? r.related_control_id.ToString() : string.Empty
            });
    }

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        int.TryParse(task.ControlId, out var ctrlId);
        int.TryParse(task.RiskId, out var riskId);
        var parameters = new DynamicParameters();
        parameters.Add("p_code", task.Code);
        parameters.Add("p_title", task.Title);
        parameters.Add("p_control_id", ctrlId > 0 ? (int?)ctrlId : null);
        parameters.Add("p_risk_id", riskId > 0 ? (int?)riskId : null);
        parameters.Add("p_owner", task.Owner);
        parameters.Add("p_priority", (int)task.Priority);
        parameters.Add("p_due_date", task.DueDate);
        parameters.Add("p_status", (int)task.Status);
        parameters.Add("p_comments", task.Comments);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_tasks_create(@p_code, @p_title, @p_control_id, @p_risk_id, @p_owner, @p_priority, @p_due_date, @p_status, @p_comments)", parameters);
        task.Id = insertedId.ToString();
        return task;
    }

    public async Task<TaskItem?> UpdateTaskAsync(TaskItem task)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", task.Id);
        parameters.Add("p_title", task.Title);
        parameters.Add("p_owner", task.Owner);
        parameters.Add("p_priority", (int)task.Priority);
        parameters.Add("p_due_date", task.DueDate);
        parameters.Add("p_status", (int)task.Status);
        parameters.Add("p_comments", task.Comments);

        var updated = await QuerySingleOrDefaultAsync<bool>("SELECT sp_tasks_update(@p_id, @p_title, @p_owner, @p_priority, @p_due_date, @p_status, @p_comments)", parameters);
        return updated ? task : null;
    }

    public async Task<bool> UpdateTaskStatusAsync(string id, ComplianceTaskStatus status)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_tasks_update_status(@id, @p_status)", new { id, p_status = (int)status });
    }

    public async Task<bool> DeleteTaskAsync(string id)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_tasks_delete(@id)", new { id });
    }
}
