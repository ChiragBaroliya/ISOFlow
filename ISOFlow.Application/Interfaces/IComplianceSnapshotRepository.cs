using ISOFlow.Application.DTOs;

namespace ISOFlow.Application.Interfaces;

/// <summary>
/// Monthly compliance history (compliance_snapshots table) that backs the dashboard trend chart.
/// <c>organizationId</c> null = platform-wide snapshot.
/// </summary>
public interface IComplianceSnapshotRepository
{
    /// <summary>Inserts or overwrites the snapshot for the month containing <paramref name="asOf"/>.</summary>
    Task UpsertMonthlySnapshotAsync(int? organizationId, DateTime asOf, double complianceScore, int openRisksCount);

    /// <summary>Returns up to <paramref name="months"/> most recent monthly snapshots, oldest first.</summary>
    Task<List<ComplianceTrendDto>> GetRecentSnapshotsAsync(int? organizationId, int months);
}
