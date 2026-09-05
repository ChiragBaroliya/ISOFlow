using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IEvidenceRepository
{
    Task<List<Evidence>> GetAllEvidenceAsync(int? organizationId);
    Task<PagedResponse<Evidence>> GetPagedEvidenceAsync(PagedRequestDto request, int? organizationId);
    Task<Evidence?> GetEvidenceByIdAsync(string id, int? organizationId);
    Task<Evidence> CreateEvidenceAsync(Evidence evidence);
    Task<Evidence?> UpdateEvidenceAsync(Evidence evidence, int organizationId);
    Task<bool> DeleteEvidenceAsync(string id, int organizationId);
}
