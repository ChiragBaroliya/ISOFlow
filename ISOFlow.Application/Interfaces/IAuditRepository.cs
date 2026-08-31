using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IAuditRepository
{
    Task<List<AuditProgram>> GetAuditProgramsAsync();
    Task<PagedResponse<AuditProgram>> GetPagedAuditProgramsAsync(PagedRequestDto request);
    Task<List<Audit>> GetAllAuditsAsync();
    Task<PagedResponse<Audit>> GetPagedAuditsAsync(PagedRequestDto request);
    Task<Audit?> GetAuditByIdAsync(string id);
    Task<Audit> CreateAuditAsync(Audit audit);
    Task<Audit?> UpdateAuditAsync(Audit audit);
    Task<bool> DeleteAuditAsync(string id);

    Task<List<Finding>> GetAllFindingsAsync();
    Task<PagedResponse<Finding>> GetPagedFindingsAsync(PagedRequestDto request);
    Task<Finding?> GetFindingByIdAsync(string id);
    Task<Finding> CreateFindingAsync(Finding finding);
    Task<Finding?> UpdateFindingAsync(Finding finding);
    Task<bool> DeleteFindingAsync(string id);
}
