using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllUsersAsync();
    Task<PagedResponse<User>> GetPagedUsersAsync(PagedRequestDto request);
    Task<List<User>> GetUsersByOrganizationIdAsync(int organizationId);
    Task<PagedResponse<User>> GetPagedUsersByOrganizationIdAsync(int organizationId, PagedRequestDto request);
    Task<User?> GetUserByIdAsync(string id);
    Task<User?> GetUserByEmailAsync(string email);

    /// <summary>Inserts the user. <see cref="User.PasswordHash"/> must already be a PBKDF2 hash.</summary>
    Task<User> CreateUserAsync(User user);
    /// <summary>Updates the user, including <see cref="User.PasswordHash"/> (which must already be hashed).</summary>
    Task<User?> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(string id);

    /// <summary>Validates email + plain-text password against the stored hash. Returns the matching user or null.</summary>
    Task<User?> ValidateLoginAsync(string email, string password);

    /// <summary>
    /// Generates a random reset token for the given email, stores only its SHA-256 hash and an
    /// expiry on the user, and returns the plain token (or null if the email was not found).
    /// </summary>
    Task<string?> GenerateResetTokenAsync(string email);

    /// <summary>
    /// Finds the user whose unexpired reset token matches <paramref name="token"/>,
    /// sets their password hash from <paramref name="newPassword"/>, and clears the token.
    /// Returns true on success, false if the token was invalid or expired.
    /// </summary>
    Task<bool> ResetPasswordAsync(string token, string newPassword);

    /// <summary>
    /// Updates editable profile fields (Name, Phone, Department, Bio, AvatarUrl) for the given user.
    /// A null <paramref name="avatarUrl"/> leaves the existing avatar untouched.
    /// Returns true on success, false if the user was not found.
    /// </summary>
    Task<bool> UpdateProfileAsync(string userId, string name, string phone, string department, string bio, string? avatarUrl = null);

    /// <summary>
    /// Validates the current password then sets the new password.
    /// Returns true on success, false if credentials don't match.
    /// </summary>
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}
