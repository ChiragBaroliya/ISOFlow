using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Web.Models;

public class DashboardViewModel
{
    public DashboardKpiDto Kpis { get; set; } = new();
    public List<ComplianceTrendDto> Trends { get; set; } = new();
    public List<RiskMatrixCellDto> RiskMatrix { get; set; } = new();
}

public class ControlDetailViewModel
{
    public Control Control { get; set; } = new();
    public RelatedItemsCountDto RelatedItems { get; set; } = new();
    public string ActiveTab { get; set; } = "overview";
}

public class RiskDetailViewModel
{
    public Risk Risk { get; set; } = new();
    public RiskTreatment? Treatment { get; set; }
}

public class TaskKanbanViewModel
{
    public List<TaskItem> ToDoTasks { get; set; } = new();
    public List<TaskItem> InProgressTasks { get; set; } = new();
    public List<TaskItem> CompletedTasks { get; set; } = new();
}
