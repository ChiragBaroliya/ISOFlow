using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.MockData;

namespace ISOFlow.Infrastructure.Repositories;

public class AuditRepository : IAuditRepository
{
    public Task<List<AuditProgram>> GetAuditProgramsAsync() => Task.FromResult(MockStore.AuditPrograms);

    public Task<List<Audit>> GetAllAuditsAsync() => Task.FromResult(MockStore.Audits);

    public Task<Audit?> GetAuditByIdAsync(string id) =>
        Task.FromResult(MockStore.Audits.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || a.Code.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<Audit> CreateAuditAsync(Audit audit)
    {
        if (string.IsNullOrWhiteSpace(audit.Id))
        {
            audit.Id = "AUD-2026-" + (MockStore.Audits.Count + 1).ToString("D3");
        }
        if (string.IsNullOrWhiteSpace(audit.Code))
        {
            audit.Code = audit.Id;
        }
        MockStore.Audits.Add(audit);
        return Task.FromResult(audit);
    }

    public Task<Audit?> UpdateAuditAsync(Audit audit)
    {
        var existing = MockStore.Audits.FirstOrDefault(a => a.Id.Equals(audit.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Title = audit.Title;
            existing.StandardId = audit.StandardId;
            existing.LeadAuditor = audit.LeadAuditor;
            existing.StartDate = audit.StartDate;
            existing.EndDate = audit.EndDate;
            existing.Status = audit.Status;
            existing.CompletionPercentage = audit.CompletionPercentage;
            existing.Scope = audit.Scope;
            existing.CheckListControlIds = audit.CheckListControlIds ?? new List<string>();
        }
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteAuditAsync(string id)
    {
        var existing = MockStore.Audits.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            MockStore.Audits.Remove(existing);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<List<Finding>> GetAllFindingsAsync() => Task.FromResult(MockStore.Findings);

    public Task<Finding?> GetFindingByIdAsync(string id) =>
        Task.FromResult(MockStore.Findings.FirstOrDefault(f => f.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || f.Code.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<Finding> CreateFindingAsync(Finding finding)
    {
        if (string.IsNullOrWhiteSpace(finding.Id))
        {
            finding.Id = MockStore.NextId("FIND", MockStore.Findings.Count);
        }
        if (string.IsNullOrWhiteSpace(finding.Code))
        {
            finding.Code = finding.Id;
        }
        if (finding.IdentifiedDate == default)
        {
            finding.IdentifiedDate = DateTime.UtcNow;
        }
        MockStore.Findings.Add(finding);
        return Task.FromResult(finding);
    }

    public Task<Finding?> UpdateFindingAsync(Finding finding)
    {
        var existing = MockStore.Findings.FirstOrDefault(f => f.Id.Equals(finding.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Title = finding.Title;
            existing.AuditId = finding.AuditId;
            existing.RequirementId = finding.RequirementId;
            existing.ControlId = finding.ControlId;
            existing.Severity = finding.Severity;
            existing.Status = finding.Status;
            existing.Description = finding.Description;
            existing.RootCause = finding.RootCause;
            existing.Auditor = finding.Auditor;
            existing.CapaId = finding.CapaId;
        }
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteFindingAsync(string id)
    {
        var existing = MockStore.Findings.FirstOrDefault(f => f.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            MockStore.Findings.Remove(existing);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
