using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IAuditRepository
{
    Task<List<AuditProgram>> GetAuditProgramsAsync();
    Task<List<Audit>> GetAllAuditsAsync();
    Task<Audit?> GetAuditByIdAsync(string id);
    Task<Audit> CreateAuditAsync(Audit audit);
    Task<Audit?> UpdateAuditAsync(Audit audit);
    Task<bool> DeleteAuditAsync(string id);

    Task<List<Finding>> GetAllFindingsAsync();
    Task<Finding?> GetFindingByIdAsync(string id);
    Task<Finding> CreateFindingAsync(Finding finding);
    Task<Finding?> UpdateFindingAsync(Finding finding);
    Task<bool> DeleteFindingAsync(string id);
}
