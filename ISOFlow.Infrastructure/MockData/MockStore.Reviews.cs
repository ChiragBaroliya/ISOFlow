using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Infrastructure.MockData;

public static partial class MockStore
{
    public static List<ManagementReview> ManagementReviews { get; } = new()
    {
        new ManagementReview
        {
            Id = "REV-2026-Q4", Code = "REV-2026-Q4",
            Title = "Management Review — Q4 2026",
            Period = "Q4 2026",
            ReviewDate = new DateTime(2026, 11, 15),
            ChairPerson = "Elena Rostova (Executive Leadership)",
            Attendees = new() { "Elena Rostova", "Chirag Baroliya", "Alex Morgan", "David Chen", "Sarah Wilson" },
            Summary = "Reviewed ISO 27001 internal audit results, high risks, open CAPAs, and approved capital investment for access lifecycle automation.",
            Status = "Completed",
            Decisions = new()
            {
                new ManagementDecision
                {
                    Id = "DEC-001", ManagementReviewId = "REV-2026-Q4",
                    DecisionText = "Automate employee lifecycle access management to eliminate unauthorized access risk.",
                    Owner = "David Chen (IT Manager)",
                    DueDate = new DateTime(2026, 12, 31),
                    ImprovementId = "IMP-001",
                    Status = "Approved"
                }
            }
        }
    };

    public static List<Improvement> Improvements { get; } = new()
    {
        new Improvement
        {
            Id = "IMP-001", Code = "IMP-001",
            Title = "Access Lifecycle Automation",
            CurrentState = "Manual email notifications for onboarding/offboarding causing delays.",
            FutureState  = "Fully automated zero-trust identity lifecycle from HR onboarding to immediate IT revocation.",
            Source = ImprovementSource.ManagementReview,
            ExpectedBenefit = "Reduce unauthorized access risk to zero and achieve 100% SLA compliance for access revocation.",
            Owner = "David Chen (IT Manager)",
            Status = ImprovementStatus.InProgress,
            RelatedReviewId  = "REV-2026-Q4",
            RelatedFindingId = "FIND-001"
        }
    };
}
