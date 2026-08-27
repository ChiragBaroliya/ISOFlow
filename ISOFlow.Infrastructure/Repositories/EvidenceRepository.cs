using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.MockData;

namespace ISOFlow.Infrastructure.Repositories;

public class EvidenceRepository : IEvidenceRepository
{
    public Task<List<Evidence>> GetAllEvidenceAsync() => Task.FromResult(MockStore.Evidences);

    public Task<Evidence?> GetEvidenceByIdAsync(string id) =>
        Task.FromResult(MockStore.Evidences.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || e.Code.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<Evidence> CreateEvidenceAsync(Evidence evidence)
    {
        if (string.IsNullOrWhiteSpace(evidence.Id))
        {
            evidence.Id = "EVI-2026-" + (MockStore.Evidences.Count + 1).ToString("D3");
        }
        if (string.IsNullOrWhiteSpace(evidence.Code))
        {
            evidence.Code = evidence.Id;
        }
        if (evidence.UploadDate == default)
        {
            evidence.UploadDate = DateTime.UtcNow;
        }
        evidence.Status = string.IsNullOrWhiteSpace(evidence.Status) ? "Verified" : evidence.Status;
        MockStore.Evidences.Add(evidence);
        return Task.FromResult(evidence);
    }

    public Task<Evidence?> UpdateEvidenceAsync(Evidence evidence)
    {
        var existing = MockStore.Evidences.FirstOrDefault(e => e.Id.Equals(evidence.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Name = evidence.Name;
            existing.Type = evidence.Type;
            existing.ControlId = evidence.ControlId;
            existing.RequirementId = evidence.RequirementId;
            existing.TaskId = evidence.TaskId;
            existing.AuditId = evidence.AuditId;
            existing.UploadedBy = evidence.UploadedBy;
            existing.ExpiryDate = evidence.ExpiryDate;
            existing.Status = evidence.Status;
            existing.FileUrl = evidence.FileUrl;
        }
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteEvidenceAsync(string id)
    {
        var existing = MockStore.Evidences.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            MockStore.Evidences.Remove(existing);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
