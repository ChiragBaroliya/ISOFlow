using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IAuditRepository
{
    Task<List<AuditProgram>> GetAuditProgramsAsync();
    Task<PagedResponse<AuditProgram>> GetPagedAuditProgramsAsync(PagedRequestDto request);
    Task<List<Audit>> GetAllAuditsAsync(int? organizationId);
    Task<PagedResponse<Audit>> GetPagedAuditsAsync(PagedRequestDto request, int? organizationId);
    Task<Audit?> GetAuditByIdAsync(string id, int? organizationId);
    Task<Audit> CreateAuditAsync(Audit audit);
    Task<Audit?> UpdateAuditAsync(Audit audit, int organizationId);
    Task<bool> DeleteAuditAsync(string id, int organizationId);

    Task<List<Finding>> GetAllFindingsAsync(int? organizationId);
    Task<PagedResponse<Finding>> GetPagedFindingsAsync(PagedRequestDto request, int? organizationId);
    Task<Finding?> GetFindingByIdAsync(string id, int? organizationId);
    Task<Finding> CreateFindingAsync(Finding finding);
    Task<Finding?> UpdateFindingAsync(Finding finding, int organizationId);
    Task<bool> DeleteFindingAsync(string id, int organizationId);
}
