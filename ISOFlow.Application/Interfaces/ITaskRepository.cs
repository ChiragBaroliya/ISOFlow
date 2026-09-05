using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Application.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllTasksAsync(int? organizationId);
    Task<PagedResponse<TaskItem>> GetPagedTasksAsync(PagedRequestDto request, int? organizationId);
    Task<TaskItem?> GetTaskByIdAsync(string id, int? organizationId);
    Task<List<TaskTemplate>> GetTaskTemplatesAsync(int? organizationId);
    Task<TaskItem> CreateTaskAsync(TaskItem task);
    Task<TaskItem?> UpdateTaskAsync(TaskItem task, int organizationId);
    Task<bool> UpdateTaskStatusAsync(string taskId, ComplianceTaskStatus status, int organizationId);
    Task<bool> DeleteTaskAsync(string id, int organizationId);
}
