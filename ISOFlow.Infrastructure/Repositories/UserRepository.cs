using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.MockData;

namespace ISOFlow.Infrastructure.Repositories;

/// <summary>
/// In-memory mock implementation of IUserRepository.
/// All mutations operate on the live MockStore.Users list (singleton per app lifetime).
///
/// MIGRATION NOTE: Replace this class with an EF Core / HttpClient implementation
/// to connect to a real database or API. The interface contract stays the same.
/// </summary>
public class UserRepository : IUserRepository
{
    // ── Queries ─────────────────────────────────────────────────────────────────

    public Task<List<User>> GetAllUsersAsync() =>
        Task.FromResult(MockStore.Users);

    public Task<List<User>> GetUsersByOrganizationIdAsync(string orgId) =>
        Task.FromResult(MockStore.Users
            .Where(u => u.OrganizationId.Equals(orgId, StringComparison.OrdinalIgnoreCase))
            .ToList());

    public Task<User?> GetUserByIdAsync(string id) =>
        Task.FromResult(MockStore.Users
            .FirstOrDefault(u => u.Id.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<User?> GetUserByEmailAsync(string email) =>
        Task.FromResult(MockStore.Users
            .FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

    public Task<User?> ValidateLoginAsync(string email, string password) =>
        Task.FromResult(MockStore.Users
            .FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password));

    // ── User Management Mutations ────────────────────────────────────────────────

    public Task<User> CreateUserAsync(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            user.Id = MockStore.NextId("USR", MockStore.Users.Count);
        }
        if (string.IsNullOrWhiteSpace(user.Password))
        {
            user.Password = "Test@123";
        }
        if (string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            user.AvatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(user.Name)}&background=4f46e5&color=fff";
        }
        MockStore.Users.Add(user);
        return Task.FromResult(user);
    }

    public Task<User?> UpdateUserAsync(User user)
    {
        var existing = MockStore.Users.FirstOrDefault(u => u.Id.Equals(user.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Name = user.Name;
            existing.Email = user.Email;
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existing.Password = user.Password;
            }
            existing.SystemRole = user.SystemRole;
            existing.Role = user.Role;
            existing.Department = user.Department;
            existing.OrganizationId = user.OrganizationId;
            existing.Location = user.Location;
            existing.Phone = user.Phone;
            existing.Bio = user.Bio;
        }
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteUserAsync(string id)
    {
        var existing = MockStore.Users.FirstOrDefault(u => u.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            MockStore.Users.Remove(existing);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    // ── Auth Mutations ───────────────────────────────────────────────────────────

    public Task<string?> GenerateResetTokenAsync(string email)
    {
        var user = MockStore.Users
            .FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (user is null) return Task.FromResult<string?>(null);

        // Generate an 8-char uppercase alphanumeric mock token
        var token = Guid.NewGuid().ToString("N")[..8].ToUpper();
        user.ResetToken = token;
        return Task.FromResult<string?>(token);
    }

    public Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var user = MockStore.Users
            .FirstOrDefault(u => u.ResetToken != null &&
                                 u.ResetToken.Equals(token, StringComparison.OrdinalIgnoreCase));

        if (user is null) return Task.FromResult(false);

        user.Password   = newPassword;
        user.ResetToken = null;
        return Task.FromResult(true);
    }

    public Task<bool> UpdateProfileAsync(string userId, string name, string phone, string department, string bio)
    {
        var user = MockStore.Users
            .FirstOrDefault(u => u.Id.Equals(userId, StringComparison.OrdinalIgnoreCase));

        if (user is null) return Task.FromResult(false);

        user.Name       = name;
        user.Phone      = phone;
        user.Department = department;
        user.Bio        = bio;
        return Task.FromResult(true);
    }

    public Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = MockStore.Users
            .FirstOrDefault(u =>
                u.Id.Equals(userId, StringComparison.OrdinalIgnoreCase) &&
                u.Password == currentPassword);

        if (user is null) return Task.FromResult(false);

        user.Password = newPassword;
        return Task.FromResult(true);
    }
}
