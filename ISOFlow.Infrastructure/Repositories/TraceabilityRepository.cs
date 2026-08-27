using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;

namespace ISOFlow.Infrastructure.Repositories;

public class TraceabilityRepository : ITraceabilityRepository
{
    public Task<TraceabilityGraphDto> GetTraceabilityGraphAsync(string rootEntityId)
    {
        // 14-Step Traceability Golden Flow Builder
        var steps = new List<TraceabilityNodeDto>
        {
            new TraceabilityNodeDto { Id = "ISO-27001-2022", Code = "ISO 27001:2022", Label = "ISO Standard", Type = "Standard", Status = "Active", Url = "/Standards/Detail?id=ISO-27001-2022", IsActive = rootEntityId.Contains("27001") },
            new TraceabilityNodeDto { Id = "REQ-A5-18", Code = "A.5.18", Label = "Access Rights Requirement", Type = "Requirement", Status = "Compliant (80%)", Url = "/Standards/Detail?id=ISO-27001-2022#A.5.18", IsActive = rootEntityId.Contains("A5") || rootEntityId.Contains("18") },
            new TraceabilityNodeDto { Id = "CTRL-001", Code = "CTRL-001", Label = "User Access Management Control", Type = "Control", Status = "Implemented", Url = "/Controls/Detail?id=CTRL-001", IsActive = rootEntityId.Contains("CTRL-001") },
            new TraceabilityNodeDto { Id = "RISK-001", Code = "RISK-001", Label = "Unauthorized Access Risk", Type = "Risk", Status = "Critical (Score: 16)", Url = "/Risks/Detail?id=RISK-001", IsActive = rootEntityId.Contains("RISK-001") },
            new TraceabilityNodeDto { Id = "TRT-001", Code = "TRT-001", Label = "Implement JML Treatment", Type = "Treatment", Status = "In Progress", Url = "/Risks/Detail?id=RISK-001", IsActive = rootEntityId.Contains("TRT-001") },
            new TraceabilityNodeDto { Id = "POL-001", Code = "POL-001", Label = "Access Control Policy", Type = "Policy", Status = "Active v2.1", Url = "/Documents/Policies", IsActive = rootEntityId.Contains("POL-001") },
            new TraceabilityNodeDto { Id = "PROC-001", Code = "PROC-001", Label = "Joiner-Mover-Leaver Process", Type = "Process", Status = "Active", Url = "/Documents/Processes", IsActive = rootEntityId.Contains("PROC-001") },
            new TraceabilityNodeDto { Id = "TASK-2026-003", Code = "TASK-2026-003", Label = "Q3 User Access Review Task", Type = "Task", Status = "Completed", Url = "/Tasks", IsActive = rootEntityId.Contains("TASK-2026-003") },
            new TraceabilityNodeDto { Id = "EVI-2026-001", Code = "EVI-2026-001", Label = "Access Review Evidence PDF", Type = "Evidence", Status = "Verified", Url = "/Evidence", IsActive = rootEntityId.Contains("EVI-2026-001") },
            new TraceabilityNodeDto { Id = "AUD-2026-001", Code = "AUD-2026-001", Label = "Internal Audit 2026", Type = "Audit", Status = "In Progress (68%)", Url = "/Audits/Detail?id=AUD-2026-001", IsActive = rootEntityId.Contains("AUD-2026-001") },
            new TraceabilityNodeDto { Id = "FIND-001", Code = "FIND-001", Label = "Access Not Removed Finding", Type = "Finding", Status = "Major NC", Url = "/Findings/Detail?id=FIND-001", IsActive = rootEntityId.Contains("FIND-001") },
            new TraceabilityNodeDto { Id = "CAPA-001", Code = "CAPA-001", Label = "Automate Access Lifecycle CAPA", Type = "CAPA", Status = "In Progress", Url = "/Capa/Detail?id=CAPA-001", IsActive = rootEntityId.Contains("CAPA-001") },
            new TraceabilityNodeDto { Id = "REV-2026-Q4", Code = "REV-2026-Q4", Label = "Q4 Management Review", Type = "Review", Status = "Completed", Url = "/ManagementReview", IsActive = rootEntityId.Contains("REV-2026-Q4") },
            new TraceabilityNodeDto { Id = "IMP-001", Code = "IMP-001", Label = "Access Lifecycle Automation Improvement", Type = "Improvement", Status = "In Progress", Url = "/Improvement", IsActive = rootEntityId.Contains("IMP-001") }
        };

        var graph = new TraceabilityGraphDto
        {
            RootId = rootEntityId,
            Steps = steps
        };

        return Task.FromResult(graph);
    }
}
