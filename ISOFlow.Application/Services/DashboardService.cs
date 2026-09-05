using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;

namespace ISOFlow.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IControlRepository _controlRepository;
    private readonly IRiskRepository _riskRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ICapaRepository _capaRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly ITraceabilityRepository _traceabilityRepository;
    private readonly ICacheService _cacheService;

    public DashboardService(
        IControlRepository controlRepository,
        IRiskRepository riskRepository,
        IAuditRepository auditRepository,
        ICapaRepository capaRepository,
        ITaskRepository taskRepository,
        IEvidenceRepository evidenceRepository,
        ITraceabilityRepository traceabilityRepository,
        ICacheService cacheService)
    {
        _controlRepository = controlRepository;
        _riskRepository = riskRepository;
        _auditRepository = auditRepository;
        _capaRepository = capaRepository;
        _taskRepository = taskRepository;
        _evidenceRepository = evidenceRepository;
        _traceabilityRepository = traceabilityRepository;
        _cacheService = cacheService;
    }

    public async Task<DashboardKpiDto> GetDashboardKpisAsync(int? organizationId)
    {
        string cacheKey = $"DashboardKpis_{(organizationId?.ToString() ?? "all")}";
        var cached = _cacheService.Get<DashboardKpiDto>(cacheKey);
        if (cached != null) return cached;

        var controls = await _controlRepository.GetAllControlsAsync(organizationId);
        var risks = await _riskRepository.GetAllRisksAsync(organizationId);
        var findings = await _auditRepository.GetAllFindingsAsync(organizationId);
        var capas = await _capaRepository.GetAllCapasAsync(organizationId);
        var tasks = await _taskRepository.GetAllTasksAsync(organizationId);
        var evidence = await _evidenceRepository.GetAllEvidenceAsync(organizationId);

        var avgCompliance = controls.Count > 0 
            ? Math.Round(controls.Average(c => c.CompliancePercentage), 1) 
            : 0.0;

        var kpi = new DashboardKpiDto
        {
            OverallCompliancePercentage = avgCompliance,
            ControlsImplementedCount = controls.Count(c => c.Status == Domain.Enums.ControlStatus.Implemented),
            ControlsTotalCount = controls.Count,
            OpenRisksCount = risks.Count(r => r.Status != "Closed" && r.Status != "Mitigated"),
            HighRisksCount = risks.Count(r => r.Level == Domain.Enums.RiskLevel.High || r.Level == Domain.Enums.RiskLevel.Critical),
            OpenFindingsCount = findings.Count(f => f.Status != Domain.Enums.FindingStatus.Closed),
            OverdueActionsCount = tasks.Count(t => t.Status == Domain.Enums.ComplianceTaskStatus.Overdue || (t.DueDate < DateTime.UtcNow && t.Status != Domain.Enums.ComplianceTaskStatus.Completed)),
            PendingEvidenceCount = evidence.Count(e => e.Status == "Pending" || e.ExpiryDate < DateTime.UtcNow.AddDays(30)),
            OpenCapaCount = capas.Count(c => c.Status != Domain.Enums.CapaStatus.Closed)
        };

        _cacheService.Set(cacheKey, kpi, TimeSpan.FromMinutes(2));
        return kpi;
    }

    public async Task<List<ComplianceTrendDto>> GetComplianceTrendsAsync(int? organizationId)
    {
        var controls = await _controlRepository.GetAllControlsAsync(organizationId);
        var risks = await _riskRepository.GetAllRisksAsync(organizationId);

        var currentScore = controls.Count > 0 ? Math.Round(controls.Average(c => c.CompliancePercentage), 1) : 85.0;
        var openRisks = risks.Count(r => r.Status != "Closed");

        var months = new[] { "Jan 2026", "Feb 2026", "Mar 2026", "Apr 2026", "May 2026", "Jun 2026", "Jul 2026", "Aug 2026" };
        var trends = new List<ComplianceTrendDto>();

        for (int i = 0; i < months.Length; i++)
        {
            var offset = (months.Length - 1 - i) * 1.8;
            var comp = Math.Max(50.0, Math.Round(currentScore - offset, 1));
            var riskVal = Math.Max(5, openRisks + (months.Length - 1 - i));
            trends.Add(new ComplianceTrendDto
            {
                Month = months[i],
                ComplianceScore = comp,
                RiskScore = riskVal
            });
        }

        return trends;
    }

    public async Task<List<RiskMatrixCellDto>> GetRiskMatrixAsync(int? organizationId)
    {
        return await _riskRepository.GetRiskMatrixDataAsync(organizationId);
    }

    public async Task<TraceabilityGraphDto> GetGoldenScenarioTraceabilityAsync(int? organizationId)
    {
        return await _traceabilityRepository.GetTraceabilityGraphAsync("CTRL-001", organizationId);
    }
}
