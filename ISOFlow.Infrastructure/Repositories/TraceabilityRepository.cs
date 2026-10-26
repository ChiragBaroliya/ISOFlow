using System.Data;
using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

/// <summary>
/// Builds the end-to-end compliance chain (Standard → … → Improvement) around a control by following the
/// real links in the database. The root may be a control code or any entity that points at a control
/// (risk, task, evidence, finding, CAPA, or an audit via its findings). Steps with no linked record are
/// omitted — nothing is invented.
/// When several records are linked at one step, the root entity itself is preferred, then the lowest id.
/// </summary>
public class TraceabilityRepository : BaseRepository, ITraceabilityRepository
{
    public TraceabilityRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public async Task<TraceabilityGraphDto> GetTraceabilityGraphAsync(string rootEntityId, int? organizationId)
    {
        var graph = new TraceabilityGraphDto { RootId = rootEntityId };
        if (string.IsNullOrWhiteSpace(rootEntityId)) return graph;

        var root = rootEntityId.Trim();
        using var conn = await DbConnectionFactory.CreateOpenConnectionAsync();

        var controlId = await ResolveControlIdAsync(conn, root, organizationId);
        if (controlId == null) return graph;

        var control = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, code, title, status, standard_id, requirement_id FROM controls WHERE id = @controlId",
            new { controlId });
        if (control == null) return graph;

        string controlCode = control.code;
        // JSON link columns store codes, e.g. '["CTRL-001","CTRL-004"]'; match the quoted code exactly.
        var codeInJson = $"%\"{controlCode}\"%";
        var p = new { controlId, root, organizationId, codeInJson };

        var requirement = control.requirement_id == null ? null : await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, clause, title, standard_id, compliance_percentage FROM requirements WHERE id = @id",
            new { id = (int)control.requirement_id });

        int? standardId = control.standard_id != null ? (int?)control.standard_id
            : requirement?.standard_id != null ? (int?)requirement.standard_id : null;
        var standard = standardId == null ? null : await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, code, name, status FROM standards WHERE id = @id", new { id = standardId });

        var risk = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, code, title, status, likelihood, impact FROM risks WHERE control_id = @controlId AND (@organizationId IS NULL OR organization_id = @organizationId) ORDER BY (LOWER(code) = LOWER(@root)) DESC, id LIMIT 1", p);

        var treatment = risk == null ? null : await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, option, status FROM risk_treatments WHERE risk_id = @riskId ORDER BY id LIMIT 1",
            new { riskId = (int)risk.id });

        var policy = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, code, title, version, status FROM policies WHERE linked_control_ids_json ILIKE @codeInJson AND (@organizationId IS NULL OR organization_id = @organizationId) ORDER BY id LIMIT 1", p);

        var process = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, code, title, status FROM processes WHERE (control_ids_json ILIKE @codeInJson OR (@policyId IS NOT NULL AND policy_id = @policyId)) AND (@organizationId IS NULL OR organization_id = @organizationId) ORDER BY id LIMIT 1",
            new { codeInJson, organizationId, policyId = policy == null ? (int?)null : (int)policy.id });

        var task = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, code, title, status FROM task_items WHERE control_id = @controlId AND (@organizationId IS NULL OR organization_id = @organizationId) ORDER BY (LOWER(code) = LOWER(@root)) DESC, id LIMIT 1", p);

        var evidence = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, code, name, status, audit_id FROM evidence WHERE control_id = @controlId AND (@organizationId IS NULL OR organization_id = @organizationId) ORDER BY (LOWER(code) = LOWER(@root)) DESC, id LIMIT 1", p);

        var finding = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT f.id, f.code, f.title, f.severity, f.status, f.audit_id FROM findings f WHERE f.control_id = @controlId AND (@organizationId IS NULL OR f.organization_id = @organizationId) ORDER BY (LOWER(f.code) = LOWER(@root) OR EXISTS (SELECT 1 FROM capas c WHERE c.finding_id = f.id AND LOWER(c.code) = LOWER(@root)) OR EXISTS (SELECT 1 FROM audits a WHERE a.id = f.audit_id AND LOWER(a.code) = LOWER(@root))) DESC, f.id LIMIT 1", p);

        int? auditId = finding?.audit_id != null ? (int?)finding.audit_id
            : evidence?.audit_id != null ? (int?)evidence.audit_id : null;
        var audit = auditId != null
            ? await conn.QueryFirstOrDefaultAsync<dynamic>("SELECT id, code, title, status, completion_percentage FROM audits WHERE id = @id", new { id = auditId })
            : await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT id, code, title, status, completion_percentage FROM audits WHERE check_list_control_ids_json ILIKE @codeInJson AND (@organizationId IS NULL OR organization_id = @organizationId) ORDER BY id LIMIT 1", p);

        dynamic? capa = null, improvement = null, review = null;
        if (finding != null)
        {
            var findingRef = new { findingId = (int)finding.id, root };
            capa = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT id, code, title, status FROM capas WHERE finding_id = @findingId ORDER BY (LOWER(code) = LOWER(@root)) DESC, id LIMIT 1", findingRef);
            improvement = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT id, code, title, status, related_review_id FROM improvements WHERE related_finding_id = @findingId ORDER BY id LIMIT 1", findingRef);
            if (improvement?.related_review_id != null)
            {
                review = await conn.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT id, code, title, status FROM management_reviews WHERE id = @id", new { id = (int)improvement.related_review_id });
            }
        }

        // Assemble in lifecycle order, skipping anything not linked.
        void Add(string type, string code, string label, string status, string url) =>
            graph.Steps.Add(new TraceabilityNodeDto
            {
                Id = code,
                Code = code,
                Label = label,
                Type = type,
                Status = status,
                Url = url,
                IsActive = string.Equals(code, root, StringComparison.OrdinalIgnoreCase)
            });

        if (standard != null)
            Add("Standard", (string)standard.code, (string)standard.name, (string)standard.status, $"/Standards/Detail?id={Uri.EscapeDataString((string)standard.code)}");
        if (requirement != null)
            Add("Requirement", (string)requirement.clause, (string)requirement.title, $"{(double)requirement.compliance_percentage:0.#}% compliant",
                standard != null ? $"/Standards/Detail?id={Uri.EscapeDataString((string)standard.code)}" : "/Standards");
        Add("Control", controlCode, (string)control.title, ((ControlStatus)(int)control.status).ToString(), $"/Controls/Detail?id={Uri.EscapeDataString(controlCode)}");
        if (risk != null)
            Add("Risk", (string)risk.code, (string)risk.title, $"{(string)risk.status} (Score: {(int)risk.likelihood * (int)risk.impact})", $"/Risks/Detail?id={Uri.EscapeDataString((string)risk.code)}");
        if (treatment != null) // only ever loaded when a risk was found
            Add("Treatment", $"TRT-{(int)treatment!.id:D3}", $"{(string)treatment.option} treatment", (string)treatment.status, $"/Risks/Detail?id={Uri.EscapeDataString((string)risk!.code)}");
        if (policy != null)
            Add("Policy", (string)policy.code, (string)policy.title, $"{(string)policy.status} v{(string)policy.version}", "/Documents/Policies");
        if (process != null)
            Add("Process", (string)process.code, (string)process.title, (string)process.status, "/Documents/Processes");
        if (task != null)
            Add("Task", (string)task.code, (string)task.title, ((ComplianceTaskStatus)(int)task.status).ToString(), "/Tasks");
        if (evidence != null)
            Add("Evidence", (string)evidence.code, (string)evidence.name, (string)evidence.status, "/Evidence");
        if (audit != null)
            Add("Audit", (string)audit.code, (string)audit.title, $"{(AuditStatus)(int)audit.status} ({(int)audit.completion_percentage}%)", $"/Audits/Detail?id={Uri.EscapeDataString((string)audit.code)}");
        if (finding != null)
            Add("Finding", (string)finding.code, (string)finding.title, $"{(FindingSeverity)(int)finding.severity} · {(FindingStatus)(int)finding.status}", $"/Findings/Detail?id={Uri.EscapeDataString((string)finding.code)}");
        if (capa != null)
            Add("CAPA", (string)capa.code, (string)capa.title, ((CapaStatus)(int)capa.status).ToString(), $"/Capa/Detail?id={Uri.EscapeDataString((string)capa.code)}");
        if (review != null)
            Add("Review", (string)review.code, (string)review.title, (string)review.status, "/ManagementReview");
        if (improvement != null)
            Add("Improvement", (string)improvement.code, (string)improvement.title, ((ImprovementStatus)(int)improvement.status).ToString(), "/Improvement");

        return graph;
    }

    /// <summary>Maps the root entity code (or a numeric control id) to the id of the control it belongs to.</summary>
    private static Task<int?> ResolveControlIdAsync(IDbConnection conn, string root, int? organizationId)
    {
        return conn.QueryFirstOrDefaultAsync<int?>(
            @"SELECT control_id FROM (
                SELECT id AS control_id, 1 AS priority FROM controls
                 WHERE (LOWER(code) = LOWER(@root) OR id::VARCHAR = @root) AND (@organizationId IS NULL OR organization_id = @organizationId)
                UNION ALL
                SELECT control_id, 2 FROM risks
                 WHERE LOWER(code) = LOWER(@root) AND (@organizationId IS NULL OR organization_id = @organizationId)
                UNION ALL
                SELECT control_id, 3 FROM task_items
                 WHERE LOWER(code) = LOWER(@root) AND (@organizationId IS NULL OR organization_id = @organizationId)
                UNION ALL
                SELECT control_id, 4 FROM evidence
                 WHERE LOWER(code) = LOWER(@root) AND (@organizationId IS NULL OR organization_id = @organizationId)
                UNION ALL
                SELECT control_id, 5 FROM findings
                 WHERE LOWER(code) = LOWER(@root) AND (@organizationId IS NULL OR organization_id = @organizationId)
                UNION ALL
                SELECT f.control_id, 6 FROM capas c JOIN findings f ON f.id = c.finding_id
                 WHERE LOWER(c.code) = LOWER(@root) AND (@organizationId IS NULL OR c.organization_id = @organizationId)
                UNION ALL
                SELECT f.control_id, 7 FROM audits a JOIN findings f ON f.audit_id = a.id
                 WHERE LOWER(a.code) = LOWER(@root) AND (@organizationId IS NULL OR a.organization_id = @organizationId)
              ) candidates
              WHERE control_id IS NOT NULL
              ORDER BY priority
              LIMIT 1",
            new { root, organizationId });
    }
}
