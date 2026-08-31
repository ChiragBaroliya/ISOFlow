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
    public UserRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<List<User>> GetAllUsersAsync()
    {
        return QueryMappedListAsync("SELECT * FROM sp_users_get_all()", r => new User
        {
            Id = r.id.ToString(),
            OrganizationId = r.organization_id != null ? r.organization_id.ToString() : string.Empty,
            Name = (string)r.name,
            Email = (string)r.email,
            Password = (string)r.password_hash,
            SystemRole = (SystemRole)(int)r.system_role,
            Role = (string)r.role,
            Department = (string)r.department,
            Location = (string)r.location,
            AvatarUrl = (string)r.avatar_url ?? string.Empty,
            Phone = (string)r.phone ?? string.Empty,
            Bio = (string)r.bio ?? string.Empty,
            ResetToken = (string?)r.reset_token
        });
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
            r => new User
            {
                Id = r.id.ToString(),
                OrganizationId = r.organization_id != null ? r.organization_id.ToString() : string.Empty,
                Name = (string)r.name,
                Email = (string)r.email,
                Password = (string)r.password_hash,
                SystemRole = (SystemRole)(int)r.system_role,
                Role = (string)r.role,
                Department = (string)r.department,
                Location = (string)r.location,
                AvatarUrl = (string)r.avatar_url ?? string.Empty,
                Phone = (string)r.phone ?? string.Empty,
                Bio = (string)r.bio ?? string.Empty,
                ResetToken = (string?)r.reset_token
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<List<User>> GetUsersByOrganizationIdAsync(string organizationId)
    {
        int.TryParse(organizationId, out var orgId);
        return QueryMappedListAsync(
            "SELECT id, organization_id, name, email, password_hash, system_role, role, department, location, avatar_url, phone, bio, reset_token FROM users WHERE organization_id = @orgId ORDER BY id ASC",
            r => new User
            {
                Id = r.id.ToString(),
                OrganizationId = r.organization_id != null ? r.organization_id.ToString() : string.Empty,
                Name = (string)r.name,
                Email = (string)r.email,
                Password = (string)r.password_hash,
                SystemRole = (SystemRole)(int)r.system_role,
                Role = (string)r.role,
                Department = (string)r.department,
                Location = (string)r.location,
                AvatarUrl = (string)r.avatar_url ?? string.Empty,
                Phone = (string)r.phone ?? string.Empty,
                Bio = (string)r.bio ?? string.Empty,
                ResetToken = (string?)r.reset_token
            },
            new { orgId });
    }

    public Task<PagedResponse<User>> GetPagedUsersByOrganizationIdAsync(string organizationId, PagedRequestDto request)
    {
        int.TryParse(organizationId, out var orgId);
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_department", (string?)null);
        parameters.Add("p_organization_id", orgId > 0 ? (int?)orgId : null);

        return QueryPagedAsync(
            "SELECT * FROM sp_users_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_department, @p_organization_id)",
            r => new User
            {
                Id = r.id.ToString(),
                OrganizationId = r.organization_id != null ? r.organization_id.ToString() : string.Empty,
                Name = (string)r.name,
                Email = (string)r.email,
                Password = (string)r.password_hash,
                SystemRole = (SystemRole)(int)r.system_role,
                Role = (string)r.role,
                Department = (string)r.department,
                Location = (string)r.location,
                AvatarUrl = (string)r.avatar_url ?? string.Empty,
                Phone = (string)r.phone ?? string.Empty,
                Bio = (string)r.bio ?? string.Empty,
                ResetToken = (string?)r.reset_token
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<User?> GetUserByIdAsync(string id)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT id, organization_id, name, email, password_hash, system_role, role, department, location, avatar_url, phone, bio, reset_token FROM users WHERE id::VARCHAR = @id",
            r => new User
            {
                Id = r.id.ToString(),
                OrganizationId = r.organization_id != null ? r.organization_id.ToString() : string.Empty,
                Name = (string)r.name,
                Email = (string)r.email,
                Password = (string)r.password_hash,
                SystemRole = (SystemRole)(int)r.system_role,
                Role = (string)r.role,
                Department = (string)r.department,
                Location = (string)r.location,
                AvatarUrl = (string)r.avatar_url ?? string.Empty,
                Phone = (string)r.phone ?? string.Empty,
                Bio = (string)r.bio ?? string.Empty,
                ResetToken = (string?)r.reset_token
            },
            new { id });
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT id, organization_id, name, email, password_hash, system_role, role, department, location, avatar_url, phone, bio, reset_token FROM users WHERE LOWER(email) = LOWER(@email)",
            r => new User
            {
                Id = r.id.ToString(),
                OrganizationId = r.organization_id != null ? r.organization_id.ToString() : string.Empty,
                Name = (string)r.name,
                Email = (string)r.email,
                Password = (string)r.password_hash,
                SystemRole = (SystemRole)(int)r.system_role,
                Role = (string)r.role,
                Department = (string)r.department,
                Location = (string)r.location,
                AvatarUrl = (string)r.avatar_url ?? string.Empty,
                Phone = (string)r.phone ?? string.Empty,
                Bio = (string)r.bio ?? string.Empty,
                ResetToken = (string?)r.reset_token
            },
            new { email });
    }

    public async Task<User> CreateUserAsync(User user)
    {
        int.TryParse(user.OrganizationId, out var orgId);
        var parameters = new DynamicParameters();
        parameters.Add("p_org_id", orgId > 0 ? (int?)orgId : null);
        parameters.Add("p_name", user.Name);
        parameters.Add("p_email", user.Email);
        parameters.Add("p_password", user.Password);
        parameters.Add("p_system_role", (int)user.SystemRole);
        parameters.Add("p_role", user.Role);
        parameters.Add("p_department", user.Department);
        parameters.Add("p_location", user.Location);
        parameters.Add("p_avatar_url", user.AvatarUrl);
        parameters.Add("p_phone", user.Phone);
        parameters.Add("p_bio", user.Bio);

        var insertedId = await QuerySingleAsync<int>("INSERT INTO users (organization_id, name, email, password_hash, system_role, role, department, location, avatar_url, phone, bio) VALUES (@p_org_id, @p_name, @p_email, @p_password, @p_system_role, @p_role, @p_department, @p_location, @p_avatar_url, @p_phone, @p_bio) RETURNING id", parameters);
        user.Id = insertedId.ToString();
        return user;
    }

    public async Task<User?> UpdateUserAsync(User user)
    {
        int.TryParse(user.OrganizationId, out var orgId);
        var parameters = new DynamicParameters();
        parameters.Add("p_id", user.Id);
        parameters.Add("p_org_id", orgId > 0 ? (int?)orgId : null);
        parameters.Add("p_name", user.Name);
        parameters.Add("p_email", user.Email);
        parameters.Add("p_system_role", (int)user.SystemRole);
        parameters.Add("p_role", user.Role);
        parameters.Add("p_department", user.Department);
        parameters.Add("p_location", user.Location);
        parameters.Add("p_avatar_url", user.AvatarUrl);
        parameters.Add("p_phone", user.Phone);
        parameters.Add("p_bio", user.Bio);

        var rows = await ExecuteAsync("UPDATE users SET organization_id = @p_org_id, name = @p_name, email = @p_email, system_role = @p_system_role, role = @p_role, department = @p_department, location = @p_location, avatar_url = @p_avatar_url, phone = @p_phone, bio = @p_bio, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE id::VARCHAR = @p_id", parameters);
        return rows > 0 ? user : null;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var rows = await ExecuteAsync("DELETE FROM users WHERE id::VARCHAR = @id", new { id });
        return rows > 0;
    }

    public Task<User?> ValidateLoginAsync(string email, string password)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_user_validate_login(@email, @password)",
            r => new User
            {
                Id = r.id.ToString(),
                OrganizationId = r.organization_id != null ? r.organization_id.ToString() : string.Empty,
                Name = (string)r.name,
                Email = (string)r.email,
                Password = password,
                SystemRole = (SystemRole)(int)r.system_role,
                Role = (string)r.role,
                Department = (string)r.department,
                Location = (string)r.location,
                AvatarUrl = (string)r.avatar_url ?? string.Empty,
                Phone = (string)r.phone ?? string.Empty,
                Bio = (string)r.bio ?? string.Empty
            },
            new { email, password });
    }

    public async Task<string?> GenerateResetTokenAsync(string email)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null) return null;

        var token = PasswordSecurityHelper.GenerateResetToken();
        await ExecuteAsync("UPDATE users SET reset_token = @token, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE LOWER(email) = LOWER(@email)", new { token, email });
        return token;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var rows = await ExecuteAsync("UPDATE users SET password_hash = @newPassword, reset_token = NULL, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE reset_token = @token", new { newPassword, token });
        return rows > 0;
    }

    public async Task<bool> UpdateProfileAsync(string id, string name, string phone, string department, string bio)
    {
        var rows = await ExecuteAsync("UPDATE users SET name = @name, phone = @phone, department = @department, bio = @bio, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE id::VARCHAR = @id", new { id, name, phone, department, bio });
        return rows > 0;
    }

    public async Task<bool> ChangePasswordAsync(string id, string currentPassword, string newPassword)
    {
        var rows = await ExecuteAsync("UPDATE users SET password_hash = @newPassword, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE id::VARCHAR = @id AND password_hash = @currentPassword", new { id, currentPassword, newPassword });
        return rows > 0;
    }
}
