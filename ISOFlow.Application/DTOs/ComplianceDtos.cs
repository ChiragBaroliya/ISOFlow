using ISOFlow.Domain.Enums;

namespace ISOFlow.Application.DTOs;

public class RelatedItemsCountDto
{
    public int Requirements { get; set; }
    public int Controls { get; set; }
    public int Risks { get; set; }
    public int Treatments { get; set; }
    public int Policies { get; set; }
    public int Processes { get; set; }
    public int Tasks { get; set; }
    public int Evidence { get; set; }
    public int Audits { get; set; }
    public int Findings { get; set; }
    public int Capa { get; set; }
    public int Improvements { get; set; }
}

public class TraceabilityNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Standard, Requirement, Control, Risk, Treatment, Policy, Process, Task, Evidence, Audit, Finding, Capa, Review, Improvement
    public string Status { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> SubNodes { get; set; } = new();
}

public class TraceabilityGraphDto
{
    public string RootId { get; set; } = string.Empty;
    public List<TraceabilityNodeDto> Steps { get; set; } = new();
}

public class DashboardKpiDto
{
    public double OverallCompliancePercentage { get; set; }
    public int ControlsImplementedCount { get; set; }
    public int ControlsTotalCount { get; set; }
    public int OpenRisksCount { get; set; }
    public int HighRisksCount { get; set; }
    public int OpenFindingsCount { get; set; }
    public int OverdueActionsCount { get; set; }
    public int PendingEvidenceCount { get; set; }
    public int OpenCapaCount { get; set; }
}

public class RiskMatrixCellDto
{
    public int Likelihood { get; set; }
    public int Impact { get; set; }
    public int Count { get; set; }
    public List<string> RiskCodes { get; set; } = new();
}

public class ComplianceTrendDto
{
    public string Month { get; set; } = string.Empty;
    public double ComplianceScore { get; set; }
    public int RiskScore { get; set; }
}
