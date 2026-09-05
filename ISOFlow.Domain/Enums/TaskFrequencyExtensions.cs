namespace ISOFlow.Domain.Enums;

public static class TaskFrequencyExtensions
{
    /// <summary>Computes the next due date for a recurring task, counted forward from <paramref name="from"/>.</summary>
    public static DateTime GetNextDueDate(this TaskFrequency frequency, DateTime from) => frequency switch
    {
        TaskFrequency.Daily => from.AddDays(1),
        TaskFrequency.Weekly => from.AddDays(7),
        TaskFrequency.Fortnightly => from.AddDays(14),
        TaskFrequency.Monthly => from.AddMonths(1),
        TaskFrequency.HalfYearly => from.AddMonths(6),
        TaskFrequency.Yearly => from.AddYears(1),
        _ => from.AddMonths(1)
    };

    public static string ToDisplayLabel(this TaskFrequency frequency) => frequency switch
    {
        TaskFrequency.HalfYearly => "Half-Yearly",
        _ => frequency.ToString()
    };
}
