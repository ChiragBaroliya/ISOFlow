using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllUsersAsync();
    Task<PagedResponse<User>> GetPagedUsersAsync(PagedRequestDto request);
    Task<List<User>> GetUsersByOrganizationIdAsync(string orgId);
    Task<PagedResponse<User>> GetPagedUsersByOrganizationIdAsync(string orgId, PagedRequestDto request);
    Task<User?> GetUserByIdAsync(string id);
    Task<User?> GetUserByEmailAsync(string email);

    Task<User> CreateUserAsync(User user);
    Task<User?> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(string id);

    /// <summary>Validates email + password. Returns the matching user or null.</summary>
    Task<User?> ValidateLoginAsync(string email, string password);

    /// <summary>
    /// Generates an 8-char mock reset token for the given email and stores it on the user.
    /// Returns the token, or null if the email was not found.
    /// </summary>
    Task<string?> GenerateResetTokenAsync(string email);

    /// <summary>
    /// Finds the user whose ResetToken matches <paramref name="token"/>,
    /// sets their password to <paramref name="newPassword"/>, and clears the token.
    /// Returns true on success, false if the token was invalid.
    /// </summary>
    Task<bool> ResetPasswordAsync(string token, string newPassword);

    /// <summary>
    /// Updates editable profile fields (Name, Phone, Department, Bio) for the given user.
    /// Returns true on success, false if the user was not found.
    /// </summary>
    Task<bool> UpdateProfileAsync(string userId, string name, string phone, string department, string bio);

    /// <summary>
    /// Validates the current password then sets the new password.
    /// Returns true on success, false if credentials don't match.
    /// </summary>
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}
