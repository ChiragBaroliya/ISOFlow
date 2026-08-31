using System.ComponentModel.DataAnnotations;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Application.DTOs;

public class StandardRequestDto
{
    [Required(ErrorMessage = "Standard Code is required (e.g. ISO 27001:2022).")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Standard Name is required.")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Revision year/version is required.")]
    [StringLength(50, ErrorMessage = "Revision cannot exceed 50 characters.")]
    public string Revision { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Range(0.0, 100.0, ErrorMessage = "Compliance percentage must be between 0 and 100.")]
    public double CompliancePercentage { get; set; } = 0.0;

    public string Status { get; set; } = "Active";
}

public class RequirementRequestDto
{
    [Required(ErrorMessage = "Standard ID is required.")]
    public string StandardId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Clause is required (e.g. 5.1).")]
    [StringLength(50, ErrorMessage = "Clause cannot exceed 50 characters.")]
    public string Clause { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
    public string Category { get; set; } = string.Empty;

    [Range(0.0, 100.0, ErrorMessage = "Compliance percentage must be between 0 and 100.")]
    public double CompliancePercentage { get; set; } = 0.0;

    public List<string> RelatedControlIds { get; set; } = new();
}

public class ControlRequestDto
{
    [Required(ErrorMessage = "Control Code is required (e.g. A.5.1).")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Control Title is required.")]
    [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
    public string Title { get; set; } = string.Empty;

    public string? RequirementId { get; set; }
    public string? StandardId { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
    public string Category { get; set; } = "Organizational Controls";

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    public ControlStatus Status { get; set; } = ControlStatus.NotImplemented;

    [Required(ErrorMessage = "Owner is required.")]
    [StringLength(150, ErrorMessage = "Owner cannot exceed 150 characters.")]
    public string Owner { get; set; } = string.Empty;

    [Range(0.0, 100.0, ErrorMessage = "Compliance percentage must be between 0 and 100.")]
    public double CompliancePercentage { get; set; } = 0.0;

    public bool IsApplicable { get; set; } = true;
    public string Justification { get; set; } = string.Empty;
}

public class RiskRequestDto
{
    [Required(ErrorMessage = "Risk Code is required (e.g. RISK-001).")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Asset is required.")]
    [StringLength(150, ErrorMessage = "Asset cannot exceed 150 characters.")]
    public string Asset { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Owner is required.")]
    [StringLength(150, ErrorMessage = "Owner cannot exceed 150 characters.")]
    public string Owner { get; set; } = string.Empty;

    [Required(ErrorMessage = "Likelihood is required.")]
    public RiskLikelihood Likelihood { get; set; } = RiskLikelihood.Possible;

    [Required(ErrorMessage = "Impact is required.")]
    public RiskImpact Impact { get; set; } = RiskImpact.Moderate;

    public string? TreatmentId { get; set; }
    public string? ControlId { get; set; }
    public string Status { get; set; } = "Open";

    public RiskTreatmentRequestDto? Treatment { get; set; }
}

public class RiskTreatmentRequestDto
{
    public string Option { get; set; } = "Mitigate";

    [Required(ErrorMessage = "Treatment Plan is required.")]
    [StringLength(2000, ErrorMessage = "Treatment plan cannot exceed 2000 characters.")]
    public string TreatmentPlan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Owner is required.")]
    public string Owner { get; set; } = string.Empty;

    [Required(ErrorMessage = "Target date is required.")]
    public DateTime TargetDate { get; set; } = DateTime.UtcNow.AddMonths(3);

    public RiskLikelihood ResidualLikelihood { get; set; } = RiskLikelihood.Unlikely;
    public RiskImpact ResidualImpact { get; set; } = RiskImpact.Minor;
    public string Status { get; set; } = "In Progress";
}

public class PolicyRequestDto
{
    [Required(ErrorMessage = "Policy Code is required (e.g. POL-001).")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Policy Title is required.")]
    [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
    public string Title { get; set; } = string.Empty;

    public string Version { get; set; } = "1.0";

    [Required(ErrorMessage = "Owner is required.")]
    public string Owner { get; set; } = string.Empty;

    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public DateTime NextReviewDate { get; set; } = DateTime.UtcNow.AddYears(1);
    public string Status { get; set; } = "Active";
    public string FilePath { get; set; } = string.Empty;

    public List<string> LinkedControlIds { get; set; } = new();
    public List<string> LinkedProcessIds { get; set; } = new();
}

public class ProcessRequestDto
{
    [Required(ErrorMessage = "Process Code is required (e.g. PROC-001).")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Process Title is required.")]
    [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Owner is required.")]
    public string Owner { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public string Status { get; set; } = "Active";

    public List<string> Steps { get; set; } = new();
    public string PolicyId { get; set; } = string.Empty;
    public List<string> ControlIds { get; set; } = new();
}

public class TaskItemRequestDto
{
    [Required(ErrorMessage = "Task Code is required.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Task Title is required.")]
    [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
    public string Title { get; set; } = string.Empty;

    public string? ControlId { get; set; }
    public string? RiskId { get; set; }

    [Required(ErrorMessage = "Owner is required.")]
    public string Owner { get; set; } = string.Empty;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [Required(ErrorMessage = "Due Date is required.")]
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);

    public ComplianceTaskStatus Status { get; set; } = ComplianceTaskStatus.NotStarted;
    public string? EvidenceId { get; set; }
    public string? Comments { get; set; }
}

public class TaskStatusUpdateDto
{
    [Required]
    public ComplianceTaskStatus Status { get; set; }
}

public class EvidenceRequestDto
{
    [Required(ErrorMessage = "Evidence Code is required.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Evidence Name is required.")]
    [StringLength(250, ErrorMessage = "Name cannot exceed 250 characters.")]
    public string Name { get; set; } = string.Empty;

    public EvidenceType Type { get; set; } = EvidenceType.Document;
    public string? ControlId { get; set; }
    public string? RequirementId { get; set; }
    public string? TaskId { get; set; }
    public string? AuditId { get; set; }

    [Required(ErrorMessage = "UploadedBy is required.")]
    public string UploadedBy { get; set; } = string.Empty;

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddYears(1);
    public string Status { get; set; } = "Verified";
    public string FileUrl { get; set; } = string.Empty;
}

public class AuditRequestDto
{
    [Required(ErrorMessage = "Audit Code is required.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Audit Title is required.")]
    [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
    public string Title { get; set; } = string.Empty;

    public string? StandardId { get; set; }

    [Required(ErrorMessage = "Lead Auditor is required.")]
    public string LeadAuditor { get; set; } = string.Empty;

    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(7);
    public AuditStatus Status { get; set; } = AuditStatus.Planned;
    public int CompletionPercentage { get; set; } = 0;
    public string Scope { get; set; } = string.Empty;

    public List<string> CheckListControlIds { get; set; } = new();
}

public class FindingRequestDto
{
    [Required(ErrorMessage = "Finding Code is required.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Finding Title is required.")]
    [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
    public string Title { get; set; } = string.Empty;

    public string? AuditId { get; set; }
    public string? RequirementId { get; set; }
    public string? ControlId { get; set; }

    public FindingSeverity Severity { get; set; } = FindingSeverity.Observation;
    public FindingStatus Status { get; set; } = FindingStatus.Open;

    public string Description { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public DateTime IdentifiedDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Auditor name is required.")]
    public string Auditor { get; set; } = string.Empty;

    public string? CapaId { get; set; }
}

public class CapaRequestDto
{
    [Required(ErrorMessage = "CAPA Code is required.")]
    public string Code { get; set; } = string.Empty;

    public string? FindingId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
    public string Title { get; set; } = string.Empty;

    public string RootCause { get; set; } = string.Empty;
    public string CorrectiveAction { get; set; } = string.Empty;

    [Required(ErrorMessage = "Owner is required.")]
    public string Owner { get; set; } = string.Empty;

    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    public CapaStatus Status { get; set; } = CapaStatus.Draft;
    public string EffectivenessVerification { get; set; } = string.Empty;
}

public class CapaActionItemRequestDto
{
    [Required(ErrorMessage = "Action item title is required.")]
    [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "AssignedTo is required.")]
    public string AssignedTo { get; set; } = string.Empty;

    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);
    public bool IsCompleted { get; set; } = false;
}

public class ManagementReviewRequestDto
{
    [Required(ErrorMessage = "Review Code is required.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
    public string Title { get; set; } = string.Empty;

    public string Period { get; set; } = "Q4 2026";
    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "ChairPerson is required.")]
    public string ChairPerson { get; set; } = string.Empty;

    public List<string> Attendees { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed";
}

public class ImprovementRequestDto
{
    [Required(ErrorMessage = "Improvement Code is required.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
    public string Title { get; set; } = string.Empty;

    public string CurrentState { get; set; } = string.Empty;
    public string FutureState { get; set; } = string.Empty;
    public ImprovementSource Source { get; set; } = ImprovementSource.ManagementReview;
    public string ExpectedBenefit { get; set; } = string.Empty;

    [Required(ErrorMessage = "Owner is required.")]
    public string Owner { get; set; } = string.Empty;

    public ImprovementStatus Status { get; set; } = ImprovementStatus.Identified;
    public string? RelatedReviewId { get; set; }
    public string? RelatedFindingId { get; set; }
}

public class OrganizationRequestDto
{
    [Required(ErrorMessage = "Organization Code is required.")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Organization Name is required.")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Industry is required.")]
    public string Industry { get; set; } = string.Empty;

    [Range(0, 1000000, ErrorMessage = "Employees must be positive.")]
    public int Employees { get; set; } = 0;

    public List<string> Locations { get; set; } = new();
    public string PrimaryStandard { get; set; } = "ISO/IEC 27001:2022";
    public string Status { get; set; } = "Active";

    [Range(0.0, 100.0, ErrorMessage = "Compliance percentage must be between 0 and 100.")]
    public double CompliancePercentage { get; set; } = 0.0;

    [Required(ErrorMessage = "Contact Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string ContactEmail { get; set; } = string.Empty;
}

public class UserRequestDto
{
    public string? OrganizationId { get; set; }

    [Required(ErrorMessage = "User Name is required.")]
    [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
    public string Password { get; set; } = "Test@123";

    public SystemRole SystemRole { get; set; } = SystemRole.User;

    [Required(ErrorMessage = "Job Role is required.")]
    public string Role { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}

public class UserLoginDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required(ErrorMessage = "Reset token is required.")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "New Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateProfileDto
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}
