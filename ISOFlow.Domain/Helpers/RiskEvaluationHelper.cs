using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Helpers;

public static class RiskEvaluationHelper
{
    public static int CalculateScore(RiskLikelihood likelihood, RiskImpact impact)
    {
        return (int)likelihood * (int)impact;
    }

    public static RiskLevel DetermineRiskLevel(int score)
    {
        return score switch
        {
            >= 15 => RiskLevel.Critical,
            >= 10 => RiskLevel.High,
            >= 5 => RiskLevel.Medium,
            _ => RiskLevel.Low
        };
    }

    public static double CalculateRiskReductionPercentage(int initialScore, int residualScore)
    {
        if (initialScore <= 0) return 0.0;
        var reduction = (double)(initialScore - residualScore) / initialScore * 100.0;
        return Math.Round(Math.Max(0.0, reduction), 1);
    }
}
