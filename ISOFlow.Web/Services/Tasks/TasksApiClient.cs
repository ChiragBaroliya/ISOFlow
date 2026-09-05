using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Tasks;

public interface ITasksApiClient
{
    Task<List<TaskItem>> GetAllTasksAsync();
    Task<TaskItem?> CreateTaskAsync(TaskItem task);
    Task<TaskItem?> UpdateTaskAsync(TaskItem task);
    Task<bool> UpdateTaskStatusAsync(string taskId, ComplianceTaskStatus status);
    Task<bool> DeleteTaskAsync(string id);

    Task<List<TaskTemplate>> GetTaskTemplatesAsync();
    Task<TaskTemplate?> CreateTaskTemplateAsync(TaskTemplate template);
    Task<TaskTemplate?> UpdateTaskTemplateAsync(TaskTemplate template);
    Task<bool> DeleteTaskTemplateAsync(string id);
    Task<TaskItem?> GenerateTaskFromTemplateAsync(string templateId);
}

public class TasksApiClient : ITasksApiClient
{
    private readonly IApiHttpClient _api;

    public TasksApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<TaskItem>> GetAllTasksAsync() =>
        await _api.GetAsync<List<TaskItem>>("api/tasks/all") ?? new();

    public async Task<TaskItem?> CreateTaskAsync(TaskItem task) =>
        await _api.PostAsync<TaskItem>("api/tasks", new
        {
            task.Code, task.Title, task.ControlId, task.RiskId,
            task.Owner, task.Priority, task.DueDate, task.Status,
            task.EvidenceId, task.Comments
        });

    public async Task<TaskItem?> UpdateTaskAsync(TaskItem task) =>
        await _api.PutAsync<TaskItem>($"api/tasks/{Uri.EscapeDataString(task.Id)}", new
        {
            task.Code, task.Title, task.ControlId, task.RiskId,
            task.Owner, task.Priority, task.DueDate, task.Status,
            task.EvidenceId, task.Comments
        });

    public async Task<bool> UpdateTaskStatusAsync(string taskId, ComplianceTaskStatus status) =>
        await _api.PatchBoolAsync($"api/tasks/{Uri.EscapeDataString(taskId)}/status", new { Status = status });

    public async Task<bool> DeleteTaskAsync(string id) =>
        await _api.DeleteAsync($"api/tasks/{Uri.EscapeDataString(id)}");

    public async Task<List<TaskTemplate>> GetTaskTemplatesAsync() =>
        await _api.GetAsync<List<TaskTemplate>>("api/tasks/templates") ?? new();

    public async Task<TaskTemplate?> CreateTaskTemplateAsync(TaskTemplate template) =>
        await _api.PostAsync<TaskTemplate>("api/tasks/templates", new
        {
            template.Code, template.Title, template.Description,
            template.Frequency, template.DefaultOwner, template.RelatedControlId
        });

    public async Task<TaskTemplate?> UpdateTaskTemplateAsync(TaskTemplate template) =>
        await _api.PutAsync<TaskTemplate>($"api/tasks/templates/{Uri.EscapeDataString(template.Id)}", new
        {
            template.Code, template.Title, template.Description,
            template.Frequency, template.DefaultOwner, template.RelatedControlId
        });

    public async Task<bool> DeleteTaskTemplateAsync(string id) =>
        await _api.DeleteAsync($"api/tasks/templates/{Uri.EscapeDataString(id)}");

    public async Task<TaskItem?> GenerateTaskFromTemplateAsync(string templateId) =>
        await _api.PostAsync<TaskItem>($"api/tasks/templates/{Uri.EscapeDataString(templateId)}/generate", new { });
}
