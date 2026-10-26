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
    public Standard? Standard { get; set; }
    public Requirement? Requirement { get; set; }
    public List<Risk> Risks { get; set; } = new();
    /// <summary>Treatments of <see cref="Risks"/>, keyed by risk id.</summary>
    public Dictionary<string, RiskTreatment> Treatments { get; set; } = new();
    public List<Policy> Policies { get; set; } = new();
    public List<Process> Processes { get; set; } = new();
    public List<TaskItem> Tasks { get; set; } = new();
    public List<Evidence> Evidence { get; set; } = new();
    public List<Audit> Audits { get; set; } = new();
    public List<Finding> Findings { get; set; } = new();
    public List<CAPA> Capas { get; set; } = new();
}

public class RiskDetailViewModel
{
    public Risk Risk { get; set; } = new();
    /// <summary>The mitigating control the risk is mapped to, if any.</summary>
    public Control? Control { get; set; }
    public RiskTreatment? Treatment { get; set; }
}

public class AuditChecklistItemViewModel
{
    /// <summary>The checklist reference as stored on the audit (control code or id).</summary>
    public string Reference { get; set; } = string.Empty;
    /// <summary>Null when the reference no longer matches a control.</summary>
    public Control? Control { get; set; }
    public List<Finding> Findings { get; set; } = new();
    public List<Evidence> Evidence { get; set; } = new();
}

public class AuditDetailViewModel
{
    public Audit Audit { get; set; } = new();
    public List<AuditChecklistItemViewModel> Checklist { get; set; } = new();
    public List<Finding> Findings { get; set; } = new();
}

public class FindingDetailViewModel
{
    public Finding Finding { get; set; } = new();
    public Control? Control { get; set; }
    public Requirement? Requirement { get; set; }
    public Audit? Audit { get; set; }
    public List<CAPA> Capas { get; set; } = new();
}

/// <summary>One dated item (task due, audit start, CAPA due) on the compliance calendar.</summary>
public class CalendarEventViewModel
{
    public DateTime Date { get; set; }
    public string Kind { get; set; } = string.Empty; // Task, Audit, CAPA
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}

public class ComplianceCalendarViewModel
{
    /// <summary>First day of the first month shown.</summary>
    public DateTime FromMonth { get; set; }
    public int MonthCount { get; set; }
    /// <summary>Open items dated before <see cref="FromMonth"/>.</summary>
    public List<CalendarEventViewModel> Overdue { get; set; } = new();
    public List<CalendarEventViewModel> Upcoming { get; set; } = new();
}

public class TaskKanbanViewModel
{
    public List<TaskItem> ToDoTasks { get; set; } = new();
    public List<TaskItem> InProgressTasks { get; set; } = new();
    public List<TaskItem> CompletedTasks { get; set; } = new();
}
