using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.MockData;

namespace ISOFlow.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    public Task<List<TaskItem>> GetAllTasksAsync() => Task.FromResult(MockStore.Tasks);

    public Task<TaskItem?> GetTaskByIdAsync(string id) =>
        Task.FromResult(MockStore.Tasks.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || t.Code.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<List<TaskTemplate>> GetTaskTemplatesAsync() => Task.FromResult(MockStore.TaskTemplates);

    public Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        if (string.IsNullOrWhiteSpace(task.Id))
        {
            task.Id = "TASK-2026-" + (MockStore.Tasks.Count + 1).ToString("D3");
        }
        if (string.IsNullOrWhiteSpace(task.Code))
        {
            task.Code = task.Id;
        }
        MockStore.Tasks.Add(task);
        return Task.FromResult(task);
    }

    public Task<TaskItem?> UpdateTaskAsync(TaskItem task)
    {
        var existing = MockStore.Tasks.FirstOrDefault(t => t.Id.Equals(task.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Title = task.Title;
            existing.ControlId = task.ControlId;
            existing.RiskId = task.RiskId;
            existing.Owner = task.Owner;
            existing.Priority = task.Priority;
            existing.DueDate = task.DueDate;
            existing.Status = task.Status;
            existing.Comments = task.Comments;
            existing.EvidenceId = task.EvidenceId;
            if (task.Status == ComplianceTaskStatus.Completed && existing.CompletedDate == null)
            {
                existing.CompletedDate = DateTime.UtcNow;
            }
        }
        return Task.FromResult(existing);
    }

    public Task<bool> UpdateTaskStatusAsync(string taskId, ComplianceTaskStatus status)
    {
        var task = MockStore.Tasks.FirstOrDefault(t => t.Id.Equals(taskId, StringComparison.OrdinalIgnoreCase) || t.Code.Equals(taskId, StringComparison.OrdinalIgnoreCase));
        if (task != null)
        {
            task.Status = status;
            if (status == ComplianceTaskStatus.Completed)
            {
                task.CompletedDate = DateTime.UtcNow;
            }
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> DeleteTaskAsync(string id)
    {
        var existing = MockStore.Tasks.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            MockStore.Tasks.Remove(existing);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
