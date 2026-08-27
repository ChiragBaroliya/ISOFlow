using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Application.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllTasksAsync();
    Task<TaskItem?> GetTaskByIdAsync(string id);
    Task<List<TaskTemplate>> GetTaskTemplatesAsync();
    Task<TaskItem> CreateTaskAsync(TaskItem task);
    Task<TaskItem?> UpdateTaskAsync(TaskItem task);
    Task<bool> UpdateTaskStatusAsync(string taskId, ComplianceTaskStatus status);
    Task<bool> DeleteTaskAsync(string id);
}
