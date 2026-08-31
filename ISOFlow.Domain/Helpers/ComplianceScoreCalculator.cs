using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Helpers;

public static class ComplianceScoreCalculator
{
    public static double CalculateStandardCompliance(IEnumerable<Requirement> requirements)
    {
        var reqList = requirements?.ToList() ?? new List<Requirement>();
        if (!reqList.Any()) return 0.0;
        return Math.Round(reqList.Average(r => r.CompliancePercentage), 1);
    }

    public static double CalculateOverallOrganizationCompliance(IEnumerable<Standard> standards)
    {
        var stdList = standards?.ToList() ?? new List<Standard>();
        if (!stdList.Any()) return 0.0;
        return Math.Round(stdList.Average(s => s.CompliancePercentage), 1);
    }

    public static ControlStatus EvaluateControlStatus(double compliancePercentage, bool isApplicable)
    {
        if (!isApplicable) return ControlStatus.NotImplemented;
        if (compliancePercentage >= 100.0) return ControlStatus.Tested;
        if (compliancePercentage >= 70.0) return ControlStatus.Implemented;
        if (compliancePercentage > 0.0) return ControlStatus.InDevelopment;
        return ControlStatus.NotImplemented;
    }
}
