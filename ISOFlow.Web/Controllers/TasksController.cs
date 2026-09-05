using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Web.Models;
using ISOFlow.Web.Services.Controls;
using ISOFlow.Web.Services.Evidence;
using ISOFlow.Web.Services.Risks;
using ISOFlow.Web.Services.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class TasksController : Controller
{
    private readonly ITasksApiClient _tasksClient;
    private readonly IControlsApiClient _controlsClient;
    private readonly IRisksApiClient _risksClient;
    private readonly IEvidenceApiClient _evidenceClient;

    public TasksController(
        ITasksApiClient tasksClient,
        IControlsApiClient controlsClient,
        IRisksApiClient risksClient,
        IEvidenceApiClient evidenceClient)
    {
        _tasksClient = tasksClient;
        _controlsClient = controlsClient;
        _risksClient = risksClient;
        _evidenceClient = evidenceClient;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Tasks";
        ViewData["ActiveTraceabilityId"] = "TASK-2026-003";
        var tasks = await _tasksClient.GetAllTasksAsync();
        ViewBag.Templates = await _tasksClient.GetTaskTemplatesAsync();
        ViewBag.Controls = await _controlsClient.GetAllControlsAsync();
        ViewBag.Risks = await _risksClient.GetAllRisksAsync();
        ViewBag.Evidence = await _evidenceClient.GetAllEvidenceAsync();
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

    // ─────────────────────────────────────────────────────────────────────────
    //  RECURRING TASK TEMPLATES
    // ─────────────────────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> CreateTemplate(TaskTemplate template)
    {
        if (ModelState.IsValid)
        {
            await _tasksClient.CreateTaskTemplateAsync(template);
            TempData["SuccessMessage"] = $"Task template '{template.Title}' created successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> EditTemplate(TaskTemplate template)
    {
        if (ModelState.IsValid)
        {
            var updated = await _tasksClient.UpdateTaskTemplateAsync(template);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"Task template '{updated.Title}' updated successfully.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTemplate(string id)
    {
        var result = await _tasksClient.DeleteTaskTemplateAsync(id);
        TempData[result ? "SuccessMessage" : "ErrorMessage"] = result ? "Task template removed successfully." : "Unable to delete task template.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> GenerateFromTemplate(string templateId)
    {
        var created = await _tasksClient.GenerateTaskFromTemplateAsync(templateId);
        if (created != null)
        {
            TempData["SuccessMessage"] = $"Task '{created.Code}' generated, due {created.DueDate:dd-MMM-yyyy}.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to generate task from template.";
        }
        return RedirectToAction(nameof(Index));
    }
}
