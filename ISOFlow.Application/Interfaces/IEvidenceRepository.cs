using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IEvidenceRepository
{
    Task<List<Evidence>> GetAllEvidenceAsync();
    Task<Evidence?> GetEvidenceByIdAsync(string id);
    Task<Evidence> CreateEvidenceAsync(Evidence evidence);
    Task<Evidence?> UpdateEvidenceAsync(Evidence evidence);
    Task<bool> DeleteEvidenceAsync(string id);
}
