using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

/// <summary>
/// Append-only persistence for the centralized audit trail. There is intentionally no
/// update/delete method: audit records must never be modifiable once written.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Inserts one audit record. When called while an ambient business transaction is active
    /// (see BaseRepository/AmbientDbContext), the insert participates in that same transaction.
    /// </summary>
    Task InsertAsync(AuditLog log);

    Task<PagedResponse<AuditLogDto>> GetPagedAsync(AuditLogFilterDto filter);

    Task<AuditLogDetailDto?> GetByIdAsync(string id);

    /// <summary>Full change history for one entity, newest first, for an entity's "History" tab.</summary>
    Task<List<AuditLogDto>> GetHistoryAsync(string entityName, string entityId);
}
