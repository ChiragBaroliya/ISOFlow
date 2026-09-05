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

    public Task<List<TaskItem>> GetAllTasksAsync(int? organizationId)
    {
        return QueryMappedListAsync(
            "SELECT * FROM sp_tasks_get_all(@p_organization_id)",
            r => new TaskItem
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
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
            new { p_organization_id = organizationId });
    }

    public Task<PagedResponse<TaskItem>> GetPagedTasksAsync(PagedRequestDto request, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", int.TryParse(request.StatusFilter, out var st) ? st : (int?)null);
        parameters.Add("p_owner", (string?)null);

        return QueryPagedAsync(
            "SELECT * FROM sp_tasks_get_paged(@p_page_number, @p_page_size, @p_organization_id, @p_search_term, @p_status, @p_owner)",
            r => new TaskItem
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
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

    public Task<TaskItem?> GetTaskByIdAsync(string id, int? organizationId)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_tasks_get_by_id(@id, @p_organization_id)",
            r => new TaskItem
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
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
            new { id, p_organization_id = organizationId });
    }

    public Task<List<TaskTemplate>> GetTaskTemplatesAsync(int? organizationId)
    {
        return QueryMappedListAsync(
            "SELECT * FROM sp_task_templates_get_all(@p_organization_id)",
            MapTemplate,
            new { p_organization_id = organizationId });
    }

    public Task<TaskTemplate?> GetTaskTemplateByIdAsync(string id, int? organizationId)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_task_templates_get_by_id(@id, @p_organization_id)",
            MapTemplate,
            new { id, p_organization_id = organizationId });
    }

    public async Task<TaskTemplate> CreateTaskTemplateAsync(TaskTemplate template)
    {
        int.TryParse(template.RelatedControlId, out var ctrlId);
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", template.OrganizationId);
        parameters.Add("p_code", template.Code);
        parameters.Add("p_title", template.Title);
        parameters.Add("p_description", template.Description);
        parameters.Add("p_frequency", (int)template.Frequency);
        parameters.Add("p_default_owner", template.DefaultOwner);
        parameters.Add("p_related_control_id", ctrlId > 0 ? (int?)ctrlId : null);

        var insertedId = await QuerySingleAsync<int>(
            "SELECT sp_task_templates_create(@p_organization_id, @p_code, @p_title, @p_description, @p_frequency, @p_default_owner, @p_related_control_id)",
            parameters);
        template.Id = insertedId.ToString();
        return template;
    }

    public async Task<TaskTemplate?> UpdateTaskTemplateAsync(TaskTemplate template, int organizationId)
    {
        int.TryParse(template.RelatedControlId, out var ctrlId);
        var parameters = new DynamicParameters();
        parameters.Add("p_id", template.Id);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_title", template.Title);
        parameters.Add("p_description", template.Description);
        parameters.Add("p_frequency", (int)template.Frequency);
        parameters.Add("p_default_owner", template.DefaultOwner);
        parameters.Add("p_related_control_id", ctrlId > 0 ? (int?)ctrlId : null);

        var updated = await QuerySingleOrDefaultAsync<bool>(
            "SELECT sp_task_templates_update(@p_id, @p_organization_id, @p_title, @p_description, @p_frequency, @p_default_owner, @p_related_control_id)",
            parameters);
        return updated ? template : null;
    }

    public async Task<bool> DeleteTaskTemplateAsync(string id, int organizationId)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_task_templates_delete(@id, @p_organization_id)", new { id, p_organization_id = organizationId });
    }

    private static TaskTemplate MapTemplate(dynamic r) => new()
    {
        Id = r.id.ToString(),
        OrganizationId = (int)r.organization_id,
        Code = (string)r.code,
        Title = (string)r.title,
        Description = (string)r.description ?? string.Empty,
        Frequency = (TaskFrequency)(int)r.frequency,
        DefaultOwner = (string)r.default_owner,
        RelatedControlId = r.related_control_id != null ? r.related_control_id.ToString() : string.Empty
    };

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        int.TryParse(task.ControlId, out var ctrlId);
        int.TryParse(task.RiskId, out var riskId);
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", task.OrganizationId);
        parameters.Add("p_code", task.Code);
        parameters.Add("p_title", task.Title);
        parameters.Add("p_control_id", ctrlId > 0 ? (int?)ctrlId : null);
        parameters.Add("p_risk_id", riskId > 0 ? (int?)riskId : null);
        parameters.Add("p_owner", task.Owner);
        parameters.Add("p_priority", (int)task.Priority);
        parameters.Add("p_due_date", task.DueDate);
        parameters.Add("p_status", (int)task.Status);
        parameters.Add("p_comments", task.Comments);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_tasks_create(@p_organization_id::INT, @p_code::VARCHAR, @p_title::VARCHAR, @p_control_id::INT, @p_risk_id::INT, @p_owner::VARCHAR, @p_priority::INT, @p_due_date::TIMESTAMP, @p_status::INT, @p_comments::TEXT)", parameters);
        task.Id = insertedId.ToString();
        return task;
    }

    public async Task<TaskItem?> UpdateTaskAsync(TaskItem task, int organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", task.Id);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_title", task.Title);
        parameters.Add("p_owner", task.Owner);
        parameters.Add("p_priority", (int)task.Priority);
        parameters.Add("p_due_date", task.DueDate);
        parameters.Add("p_status", (int)task.Status);
        parameters.Add("p_comments", task.Comments);

        var updated = await QuerySingleOrDefaultAsync<bool>("SELECT sp_tasks_update(@p_id::VARCHAR, @p_organization_id::INT, @p_title::VARCHAR, @p_owner::VARCHAR, @p_priority::INT, @p_due_date::TIMESTAMP, @p_status::INT, @p_comments::TEXT)", parameters);
        return updated ? task : null;
    }

    public async Task<bool> UpdateTaskStatusAsync(string id, ComplianceTaskStatus status, int organizationId)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_tasks_update_status(@id::VARCHAR, @p_organization_id::INT, @p_status::INT)", new { id, p_organization_id = organizationId, p_status = (int)status });
    }

    public async Task<bool> DeleteTaskAsync(string id, int organizationId)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_tasks_delete(@id, @p_organization_id)", new { id, p_organization_id = organizationId });
    }
}
