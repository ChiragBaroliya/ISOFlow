using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Helpers;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class UserRepository : BaseRepository, IUserRepository
{
    private const string UserColumns =
        "id, organization_id, name, email, password_hash, system_role, role, department, location, avatar_url, phone, bio, reset_token, reset_token_expires_at";

    public UserRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    /// <summary>Maps the columns common to every user query (stored procedures included).</summary>
    private static User MapUser(dynamic r) => new()
    {
        Id = r.id.ToString(),
        OrganizationId = r.organization_id == null ? (int?)null : (int)r.organization_id,
        Name = (string)r.name,
        Email = (string)r.email,
        PasswordHash = (string)r.password_hash,
        SystemRole = (SystemRole)(int)r.system_role,
        Role = (string)r.role,
        Department = (string)r.department,
        Location = (string)r.location,
        AvatarUrl = (string)r.avatar_url ?? string.Empty,
        Phone = (string)r.phone ?? string.Empty,
        Bio = (string)r.bio ?? string.Empty
    };

    /// <summary>Maps a full <see cref="UserColumns"/> row, including the pending reset-token state.</summary>
    private static User MapUserWithResetState(dynamic r)
    {
        User user = MapUser(r);
        user.ResetTokenHash = (string?)r.reset_token;
        user.ResetTokenExpiresAt = (DateTime?)r.reset_token_expires_at;
        return user;
    }

    public Task<List<User>> GetAllUsersAsync()
    {
        return QueryMappedListAsync("SELECT * FROM sp_users_get_all()", MapUser);
    }

    public Task<PagedResponse<User>> GetPagedUsersAsync(PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_department", string.IsNullOrWhiteSpace(request.CategoryFilter) ? null : request.CategoryFilter.Trim());
        parameters.Add("p_organization_id", (int?)null);

        return QueryPagedAsync(
            "SELECT * FROM sp_users_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_department, @p_organization_id)",
            MapUser,
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<List<User>> GetUsersByOrganizationIdAsync(int organizationId)
    {
        return QueryMappedListAsync(
            $"SELECT {UserColumns} FROM users WHERE organization_id = @organizationId ORDER BY id ASC",
            MapUser,
            new { organizationId });
    }

    public Task<PagedResponse<User>> GetPagedUsersByOrganizationIdAsync(int organizationId, PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_department", (string?)null);
        parameters.Add("p_organization_id", organizationId);

        return QueryPagedAsync(
            "SELECT * FROM sp_users_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_department, @p_organization_id)",
            MapUser,
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<User?> GetUserByIdAsync(string id)
    {
        return QueryMappedFirstOrDefaultAsync(
            $"SELECT {UserColumns} FROM users WHERE id::VARCHAR = @id",
            MapUserWithResetState,
            new { id });
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        return QueryMappedFirstOrDefaultAsync(
            $"SELECT {UserColumns} FROM users WHERE LOWER(email) = LOWER(@email)",
            MapUserWithResetState,
            new { email });
    }

    public async Task<User> CreateUserAsync(User user)
    {
        if (!PasswordSecurityHelper.IsPasswordHash(user.PasswordHash))
            throw new ArgumentException("User.PasswordHash must be a PBKDF2 hash, never a plain-text password.", nameof(user));

        var parameters = new DynamicParameters();
        parameters.Add("p_org_id", user.OrganizationId);
        parameters.Add("p_name", user.Name);
        parameters.Add("p_email", user.Email);
        parameters.Add("p_password_hash", user.PasswordHash);
        parameters.Add("p_system_role", (int)user.SystemRole);
        parameters.Add("p_role", user.Role);
        parameters.Add("p_department", user.Department);
        parameters.Add("p_location", user.Location);
        parameters.Add("p_avatar_url", user.AvatarUrl);
        parameters.Add("p_phone", user.Phone);
        parameters.Add("p_bio", user.Bio);

        var insertedId = await QuerySingleAsync<int>("INSERT INTO users (organization_id, name, email, password_hash, system_role, role, department, location, avatar_url, phone, bio) VALUES (@p_org_id, @p_name, @p_email, @p_password_hash, @p_system_role, @p_role, @p_department, @p_location, @p_avatar_url, @p_phone, @p_bio) RETURNING id", parameters);
        user.Id = insertedId.ToString();
        return user;
    }

    public async Task<User?> UpdateUserAsync(User user)
    {
        if (!PasswordSecurityHelper.IsPasswordHash(user.PasswordHash))
            throw new ArgumentException("User.PasswordHash must be a PBKDF2 hash, never a plain-text password.", nameof(user));

        var parameters = new DynamicParameters();
        parameters.Add("p_id", user.Id);
        parameters.Add("p_org_id", user.OrganizationId);
        parameters.Add("p_name", user.Name);
        parameters.Add("p_email", user.Email);
        parameters.Add("p_password_hash", user.PasswordHash);
        parameters.Add("p_system_role", (int)user.SystemRole);
        parameters.Add("p_role", user.Role);
        parameters.Add("p_department", user.Department);
        parameters.Add("p_location", user.Location);
        parameters.Add("p_avatar_url", user.AvatarUrl);
        parameters.Add("p_phone", user.Phone);
        parameters.Add("p_bio", user.Bio);

        var rows = await ExecuteAsync("UPDATE users SET organization_id = @p_org_id, name = @p_name, email = @p_email, password_hash = @p_password_hash, system_role = @p_system_role, role = @p_role, department = @p_department, location = @p_location, avatar_url = @p_avatar_url, phone = @p_phone, bio = @p_bio, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE id::VARCHAR = @p_id", parameters);
        return rows > 0 ? user : null;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var rows = await ExecuteAsync("DELETE FROM users WHERE id::VARCHAR = @id", new { id });
        return rows > 0;
    }

    public async Task<User?> ValidateLoginAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);
        return user != null && PasswordSecurityHelper.VerifyPassword(password, user.PasswordHash) ? user : null;
    }

    public async Task<string?> GenerateResetTokenAsync(string email)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null) return null;

        var token = PasswordSecurityHelper.GenerateResetToken();
        var tokenHash = PasswordSecurityHelper.HashResetToken(token);
        var expiresAt = DateTime.UtcNow.Add(PasswordSecurityHelper.ResetTokenLifetime);

        await ExecuteAsync(
            "UPDATE users SET reset_token = @tokenHash, reset_token_expires_at = @expiresAt, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE id = @id",
            new { tokenHash, expiresAt, id = int.Parse(user.Id) });
        return token;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        var tokenHash = PasswordSecurityHelper.HashResetToken(token);
        var newPasswordHash = PasswordSecurityHelper.HashPassword(newPassword);

        var rows = await ExecuteAsync(
            "UPDATE users SET password_hash = @newPasswordHash, reset_token = NULL, reset_token_expires_at = NULL, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE reset_token = @tokenHash AND reset_token_expires_at > @now",
            new { newPasswordHash, tokenHash, now = DateTime.UtcNow });
        return rows > 0;
    }

    public async Task<bool> UpdateProfileAsync(string id, string name, string phone, string department, string bio, string? avatarUrl = null)
    {
        var sql = "UPDATE users SET name = @name, phone = @phone, department = @department, bio = @bio, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata')"
            + (avatarUrl != null ? ", avatar_url = @avatarUrl" : "")
            + " WHERE id::VARCHAR = @id";
        var rows = await ExecuteAsync(sql, new { id, name, phone, department, bio, avatarUrl });
        return rows > 0;
    }

    public async Task<bool> ChangePasswordAsync(string id, string currentPassword, string newPassword)
    {
        var user = await GetUserByIdAsync(id);
        if (user == null || !PasswordSecurityHelper.VerifyPassword(currentPassword, user.PasswordHash))
            return false;

        var newPasswordHash = PasswordSecurityHelper.HashPassword(newPassword);

        // Guard on the hash we verified against so a concurrent change can't be silently overwritten.
        var rows = await ExecuteAsync(
            "UPDATE users SET password_hash = @newPasswordHash, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE id::VARCHAR = @id AND password_hash = @currentHash",
            new { id, newPasswordHash, currentHash = user.PasswordHash });
        return rows > 0;
    }
}
