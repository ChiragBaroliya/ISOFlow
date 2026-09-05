using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Entities;

public class Improvement
{
    public string Id { get; set; } = string.Empty; // IMP-001
    public int OrganizationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CurrentState { get; set; } = string.Empty;
    public string FutureState { get; set; } = string.Empty;
    public ImprovementSource Source { get; set; }
    public string ExpectedBenefit { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public ImprovementStatus Status { get; set; }
    public string RelatedReviewId { get; set; } = string.Empty;
    public string RelatedFindingId { get; set; } = string.Empty;
}
