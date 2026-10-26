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
    private readonly IComplianceSnapshotRepository _snapshotRepository;
    private readonly ICacheService _cacheService;

    /// <summary>How many months of history the trend chart shows.</summary>
    private const int TrendMonths = 12;

    public DashboardService(
        IControlRepository controlRepository,
        IRiskRepository riskRepository,
        IAuditRepository auditRepository,
        ICapaRepository capaRepository,
        ITaskRepository taskRepository,
        IEvidenceRepository evidenceRepository,
        ITraceabilityRepository traceabilityRepository,
        IComplianceSnapshotRepository snapshotRepository,
        ICacheService cacheService)
    {
        _controlRepository = controlRepository;
        _riskRepository = riskRepository;
        _auditRepository = auditRepository;
        _capaRepository = capaRepository;
        _taskRepository = taskRepository;
        _evidenceRepository = evidenceRepository;
        _traceabilityRepository = traceabilityRepository;
        _snapshotRepository = snapshotRepository;
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

    /// <summary>
    /// Records the current month's live compliance score / open-risk count as a snapshot, then returns
    /// the stored monthly history. History therefore starts on the first month the dashboard is used;
    /// earlier months are never fabricated.
    /// </summary>
    public async Task<List<ComplianceTrendDto>> GetComplianceTrendsAsync(int? organizationId)
    {
        var controls = await _controlRepository.GetAllControlsAsync(organizationId);
        var risks = await _riskRepository.GetAllRisksAsync(organizationId);

        var currentScore = controls.Count > 0 ? Math.Round(controls.Average(c => c.CompliancePercentage), 1) : 0.0;
        var openRisks = risks.Count(r => r.Status != "Closed" && r.Status != "Mitigated");

        await _snapshotRepository.UpsertMonthlySnapshotAsync(organizationId, DateTime.UtcNow, currentScore, openRisks);
        return await _snapshotRepository.GetRecentSnapshotsAsync(organizationId, TrendMonths);
    }

    public async Task<List<RiskMatrixCellDto>> GetRiskMatrixAsync(int? organizationId)
    {
        return await _riskRepository.GetRiskMatrixDataAsync(organizationId);
    }

    public async Task<TraceabilityGraphDto> GetControlTraceabilityAsync(string controlId, int? organizationId)
    {
        return await _traceabilityRepository.GetTraceabilityGraphAsync(controlId, organizationId);
    }
}
