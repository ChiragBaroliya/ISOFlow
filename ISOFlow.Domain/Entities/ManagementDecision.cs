namespace ISOFlow.Domain.Entities;

public class ManagementDecision
{
    public string Id { get; set; } = string.Empty;
    public string ManagementReviewId { get; set; } = string.Empty;
    public string DecisionText { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string ImprovementId { get; set; } = string.Empty;
    public string Status { get; set; } = "Approved";
}
