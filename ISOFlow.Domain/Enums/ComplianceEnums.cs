namespace ISOFlow.Domain.Enums;

public enum ControlStatus
{
    NotImplemented,
    InDevelopment,
    Implemented,
    Tested,
    NeedsReview
}

public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}

public enum RiskLikelihood
{
    Rare = 1,
    Unlikely = 2,
    Possible = 3,
    Likely = 4,
    AlmostCertain = 5
}

public enum RiskImpact
{
    Negligible = 1,
    Minor = 2,
    Moderate = 3,
    Major = 4,
    Critical = 5
}

public enum FindingSeverity
{
    OpportunityForImprovement,
    Observation,
    MinorNonConformity,
    MajorNonConformity
}

public enum FindingStatus
{
    Open,
    InAnalysis,
    CapaAssigned,
    Resolved,
    Closed
}

public enum CapaStatus
{
    Draft,
    InAnalysis,
    ActionPlanApproved,
    InProgress,
    VerificationPending,
    Closed
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum ComplianceTaskStatus
{
    NotStarted,
    InProgress,
    Blocked,
    Completed,
    Overdue
}

public enum AuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Reporting,
    Completed
}

public enum EvidenceType
{
    Document,
    Report,
    ApprovalRecord,
    LogFile,
    Screenshot,
    PolicyDocument
}

public enum ImprovementSource
{
    AuditFinding,
    RiskAssessment,
    Incident,
    ManagementReview,
    ComplianceReview,
    EmployeeSuggestion
}

public enum ImprovementStatus
{
    Identified,
    Approved,
    InProgress,
    Completed,
    Verified
}

public enum PolicyStatus
{
    Draft,
    UnderReview,
    Active,
    Archived
}

public enum RiskTreatmentOption
{
    Mitigate,
    Avoid,
    Transfer,
    Accept
}

public enum NotificationCategory
{
    General,
    Warning,
    Task,
    Policy,
    Audit
}

/// <summary>Recurrence cadence for a <see cref="Entities.TaskTemplate"/>.</summary>
public enum TaskFrequency
{
    Daily,
    Weekly,
    Fortnightly,
    Monthly,
    HalfYearly,
    Yearly
}
