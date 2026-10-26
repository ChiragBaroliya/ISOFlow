using System.Text.Json.Serialization;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Entities;

public class User
{
    public string Id { get; set; } = string.Empty;
    /// <summary>Organization this user belongs to. Null for SuperAdmin.</summary>
    public int? OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    /// <summary>PBKDF2 password hash (see PasswordSecurityHelper). Never serialized to API clients.</summary>
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>System-level role: SuperAdmin (global), Admin (org-scoped), User (org-scoped).</summary>
    public SystemRole SystemRole { get; set; } = SystemRole.User;
    /// <summary>Job/functional role label (e.g. Compliance Manager, CISO).</summary>
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    /// <summary>Contact phone number.</summary>
    public string Phone { get; set; } = string.Empty;
    /// <summary>Short biography shown on the profile page.</summary>
    public string Bio { get; set; } = string.Empty;
    /// <summary>SHA-256 hash of the pending password-reset token. Null = no pending reset.</summary>
    [JsonIgnore]
    public string? ResetTokenHash { get; set; }
    /// <summary>When the pending password-reset token stops being accepted.</summary>
    [JsonIgnore]
    public DateTime? ResetTokenExpiresAt { get; set; }
}
