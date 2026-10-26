using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class ComplianceSnapshotRepository : BaseRepository, IComplianceSnapshotRepository
{
    public ComplianceSnapshotRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public async Task UpsertMonthlySnapshotAsync(int? organizationId, DateTime asOf, double complianceScore, int openRisksCount)
    {
        var snapshotMonth = new DateTime(asOf.Year, asOf.Month, 1);

        await ExecuteAsync(
            @"INSERT INTO compliance_snapshots (organization_id, snapshot_month, compliance_score, open_risks_count, captured_at)
              VALUES (@organizationId, @snapshotMonth, @complianceScore, @openRisksCount, CURRENT_TIMESTAMP)
              ON CONFLICT ((COALESCE(organization_id, 0)), snapshot_month)
              DO UPDATE SET compliance_score = EXCLUDED.compliance_score,
                            open_risks_count = EXCLUDED.open_risks_count,
                            captured_at = EXCLUDED.captured_at",
            new { organizationId, snapshotMonth, complianceScore, openRisksCount });
    }

    public async Task<List<ComplianceTrendDto>> GetRecentSnapshotsAsync(int? organizationId, int months)
    {
        var recent = await QueryMappedListAsync(
            @"SELECT snapshot_month, compliance_score, open_risks_count
              FROM compliance_snapshots
              WHERE COALESCE(organization_id, 0) = COALESCE(@organizationId, 0)
              ORDER BY snapshot_month DESC
              LIMIT @months",
            r => new ComplianceTrendDto
            {
                Month = ((DateTime)r.snapshot_month).ToString("MMM yyyy"),
                ComplianceScore = Math.Round((double)r.compliance_score, 1),
                RiskScore = (int)r.open_risks_count
            },
            new { organizationId, months });

        recent.Reverse();
        return recent;
    }
}
