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

    /// <param name="organizationId">
    /// Optional tenant filter. Null means "no filter" — callers must pass a concrete value for
    /// every non-SuperAdmin caller; only a SuperAdmin caller should ever leave this null.
    /// </param>
    Task<PagedResponse<AuditLogDto>> GetPagedAsync(AuditLogFilterDto filter, int? organizationId = null);

    /// <param name="organizationId">Optional tenant filter (see <see cref="GetPagedAsync"/>).</param>
    Task<AuditLogDetailDto?> GetByIdAsync(string id, int? organizationId = null);

    /// <summary>Full change history for one entity, newest first, for an entity's "History" tab.</summary>
    /// <param name="organizationId">Optional tenant filter (see <see cref="GetPagedAsync"/>).</param>
    Task<List<AuditLogDto>> GetHistoryAsync(string entityName, string entityId, int? organizationId = null);
}
