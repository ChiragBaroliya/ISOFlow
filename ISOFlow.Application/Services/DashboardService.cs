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

    public async Task<DashboardKpiDto> GetDashboardKpisAsync()
    {
        const string cacheKey = "DashboardKpis";
        var cached = _cacheService.Get<DashboardKpiDto>(cacheKey);
        if (cached != null) return cached;

        var controls = await _controlRepository.GetAllControlsAsync();
        var risks = await _riskRepository.GetAllRisksAsync();
        var findings = await _auditRepository.GetAllFindingsAsync();
        var capas = await _capaRepository.GetAllCapasAsync();
        var tasks = await _taskRepository.GetAllTasksAsync();
        var evidence = await _evidenceRepository.GetAllEvidenceAsync();

        var kpi = new DashboardKpiDto
        {
            OverallCompliancePercentage = 82.5,
            ControlsImplementedCount = controls.Count(c => c.Status == Domain.Enums.ControlStatus.Implemented),
            ControlsTotalCount = controls.Count,
            OpenRisksCount = risks.Count(r => r.Status != "Closed"),
            HighRisksCount = risks.Count(r => r.Level == Domain.Enums.RiskLevel.High || r.Level == Domain.Enums.RiskLevel.Critical),
            OpenFindingsCount = findings.Count(f => f.Status != Domain.Enums.FindingStatus.Closed),
            OverdueActionsCount = tasks.Count(t => t.Status == Domain.Enums.ComplianceTaskStatus.Overdue || (t.DueDate < DateTime.UtcNow && t.Status != Domain.Enums.ComplianceTaskStatus.Completed)),
            PendingEvidenceCount = evidence.Count(e => e.Status == "Pending" || e.ExpiryDate < DateTime.UtcNow.AddDays(30)),
            OpenCapaCount = capas.Count(c => c.Status != Domain.Enums.CapaStatus.Closed)
        };

        _cacheService.Set(cacheKey, kpi, TimeSpan.FromMinutes(5));
        return kpi;
    }

    public Task<List<ComplianceTrendDto>> GetComplianceTrendsAsync()
    {
        var trends = new List<ComplianceTrendDto>
        {
            new ComplianceTrendDto { Month = "Jan 2026", ComplianceScore = 72.0, RiskScore = 28 },
            new ComplianceTrendDto { Month = "Feb 2026", ComplianceScore = 74.5, RiskScore = 26 },
            new ComplianceTrendDto { Month = "Mar 2026", ComplianceScore = 76.0, RiskScore = 24 },
            new ComplianceTrendDto { Month = "Apr 2026", ComplianceScore = 78.0, RiskScore = 21 },
            new ComplianceTrendDto { Month = "May 2026", ComplianceScore = 79.5, RiskScore = 19 },
            new ComplianceTrendDto { Month = "Jun 2026", ComplianceScore = 81.0, RiskScore = 17 },
            new ComplianceTrendDto { Month = "Jul 2026", ComplianceScore = 82.5, RiskScore = 16 }
        };
        return Task.FromResult(trends);
    }

    public async Task<List<RiskMatrixCellDto>> GetRiskMatrixAsync()
    {
        return await _riskRepository.GetRiskMatrixDataAsync();
    }

    public async Task<TraceabilityGraphDto> GetGoldenScenarioTraceabilityAsync()
    {
        return await _traceabilityRepository.GetTraceabilityGraphAsync("CTRL-001");
    }
}
