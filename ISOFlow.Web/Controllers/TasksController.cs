using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Web.Models;
using ISOFlow.Web.Services.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class TasksController : Controller
{
    private readonly ITasksApiClient _tasksClient;

    public TasksController(ITasksApiClient tasksClient)
    {
        _tasksClient = tasksClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Tasks";
        ViewData["ActiveTraceabilityId"] = "TASK-2026-003";
        var tasks = await _tasksClient.GetAllTasksAsync();
        return View(tasks);
    }

    public async Task<IActionResult> Kanban()
    {
        ViewData["ActiveMenu"] = "Kanban";
        ViewData["ActiveTraceabilityId"] = "TASK-2026-003";
        var tasks = await _tasksClient.GetAllTasksAsync();

        var vm = new TaskKanbanViewModel
        {
            ToDoTasks = tasks.Where(t => t.Status == ComplianceTaskStatus.NotStarted).ToList(),
            InProgressTasks = tasks.Where(t => t.Status == ComplianceTaskStatus.InProgress).ToList(),
            CompletedTasks = tasks.Where(t => t.Status == ComplianceTaskStatus.Completed).ToList()
        };

        return View(vm);
    }

    public IActionResult Calendar()
    {
        ViewData["ActiveMenu"] = "Calendar";
        ViewData["ActiveTraceabilityId"] = "TASK-2026-003";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(TaskItem task)
    {
        if (ModelState.IsValid)
        {
            await _tasksClient.CreateTaskAsync(task);
            TempData["SuccessMessage"] = $"Task '{task.Title}' created successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TaskItem task)
    {
        if (ModelState.IsValid)
        {
            var updated = await _tasksClient.UpdateTaskAsync(task);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Task '{updated.Title}' updated successfully.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(string taskId, ComplianceTaskStatus status, string? returnUrl = null)
    {
        await _tasksClient.UpdateTaskStatusAsync(taskId, status);
        TempData["SuccessMessage"] = $"Task status updated to {status}.";
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _tasksClient.DeleteTaskAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Task removed successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete task.";
        }
        return RedirectToAction(nameof(Index));
    }
}
