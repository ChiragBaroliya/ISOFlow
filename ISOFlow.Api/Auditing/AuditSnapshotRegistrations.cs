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
        registry.Register("Control", async (sp, id, organizationId) =>
            (object?)await sp.GetRequiredService<IControlRepository>().GetControlByIdAsync(id, organizationId));

        registry.Register("Risk", async (sp, id, organizationId) =>
            (object?)await sp.GetRequiredService<IRiskRepository>().GetRiskByIdAsync(id, organizationId));

        registry.Register("Standard", async (sp, id, organizationId) =>
            (object?)await sp.GetRequiredService<IStandardRepository>().GetStandardByIdAsync(id, organizationId));

        registry.Register("Requirement", async (sp, id, organizationId) =>
            (object?)await sp.GetRequiredService<IStandardRepository>().GetRequirementByIdAsync(id, organizationId));

        registry.Register("Policy", async (sp, id, organizationId) =>
            (object?)await sp.GetRequiredService<IDocumentRepository>().GetPolicyByIdAsync(id, organizationId));

        registry.Register("Process", async (sp, id, organizationId) =>
            (object?)await sp.GetRequiredService<IDocumentRepository>().GetProcessByIdAsync(id, organizationId));

        registry.Register("Task", async (sp, id, organizationId) =>
            (object?)await sp.GetRequiredService<ITaskRepository>().GetTaskByIdAsync(id, organizationId));

        registry.Register("Evidence", async (sp, id, organizationId) =>
            (object?)await sp.GetRequiredService<IEvidenceRepository>().GetEvidenceByIdAsync(id, organizationId));

        registry.Register("Audit", async (sp, id, organizationId) =>
            (object?)await sp.GetRequiredService<IAuditRepository>().GetAuditByIdAsync(id, organizationId));

        registry.Register("Finding", async (sp, id, organizationId) =>
            (object?)await sp.GetRequiredService<IAuditRepository>().GetFindingByIdAsync(id, organizationId));

        registry.Register("Capa", async (sp, id, organizationId) =>
            (object?)await sp.GetRequiredService<ICapaRepository>().GetCapaByIdAsync(id, organizationId));

        registry.Register("ManagementReview", async (sp, id, organizationId) =>
            (object?)await sp.GetRequiredService<IManagementReviewRepository>().GetReviewByIdAsync(id, organizationId));

        registry.Register("Improvement", async (sp, id, organizationId) =>
            (object?)await sp.GetRequiredService<IManagementReviewRepository>().GetImprovementByIdAsync(id, organizationId));

        // Organization and User are not organization-scoped entities themselves — their GetById
        // methods take no organizationId parameter, so it's simply ignored here.
        registry.Register("Organization", async (sp, id, _) =>
            (object?)await sp.GetRequiredService<IOrganizationRepository>().GetOrganizationByIdAsync(id));

        registry.Register("User", async (sp, id, _) =>
            (object?)await sp.GetRequiredService<IUserRepository>().GetUserByIdAsync(id));
    }
}
