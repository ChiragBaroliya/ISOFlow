using ISOFlow.Application.Interfaces;

namespace ISOFlow.Api.Auditing;

/// <summary>
/// The one place a new entity type needs a single line of wiring to participate in automatic
/// before/after audit snapshotting — everything else (diffing, redaction, JSON, writing, sharing
/// the business transaction) is handled generically by <see cref="AuditActionFilter"/> and the
/// registered <c>IAuditLogService</c>. Each entry just points at that entity's own repository
/// GetById method; no bespoke audit code is written per entity.
/// </summary>
public static class AuditSnapshotRegistrations
{
    public static void RegisterAll(IAuditSnapshotRegistry registry)
    {
        registry.Register("Control", async (sp, id) =>
            (object?)await sp.GetRequiredService<IControlRepository>().GetControlByIdAsync(id));

        registry.Register("Risk", async (sp, id) =>
            (object?)await sp.GetRequiredService<IRiskRepository>().GetRiskByIdAsync(id));

        registry.Register("Standard", async (sp, id) =>
            (object?)await sp.GetRequiredService<IStandardRepository>().GetStandardByIdAsync(id));

        registry.Register("Requirement", async (sp, id) =>
            (object?)await sp.GetRequiredService<IStandardRepository>().GetRequirementByIdAsync(id));

        registry.Register("Policy", async (sp, id) =>
            (object?)await sp.GetRequiredService<IDocumentRepository>().GetPolicyByIdAsync(id));

        registry.Register("Process", async (sp, id) =>
            (object?)await sp.GetRequiredService<IDocumentRepository>().GetProcessByIdAsync(id));

        registry.Register("Task", async (sp, id) =>
            (object?)await sp.GetRequiredService<ITaskRepository>().GetTaskByIdAsync(id));

        registry.Register("Evidence", async (sp, id) =>
            (object?)await sp.GetRequiredService<IEvidenceRepository>().GetEvidenceByIdAsync(id));

        registry.Register("Audit", async (sp, id) =>
            (object?)await sp.GetRequiredService<IAuditRepository>().GetAuditByIdAsync(id));

        registry.Register("Finding", async (sp, id) =>
            (object?)await sp.GetRequiredService<IAuditRepository>().GetFindingByIdAsync(id));

        registry.Register("Capa", async (sp, id) =>
            (object?)await sp.GetRequiredService<ICapaRepository>().GetCapaByIdAsync(id));

        registry.Register("ManagementReview", async (sp, id) =>
            (object?)await sp.GetRequiredService<IManagementReviewRepository>().GetReviewByIdAsync(id));

        registry.Register("Improvement", async (sp, id) =>
            (object?)await sp.GetRequiredService<IManagementReviewRepository>().GetImprovementByIdAsync(id));

        registry.Register("Organization", async (sp, id) =>
            (object?)await sp.GetRequiredService<IOrganizationRepository>().GetOrganizationByIdAsync(id));

        registry.Register("User", async (sp, id) =>
            (object?)await sp.GetRequiredService<IUserRepository>().GetUserByIdAsync(id));
    }
}
