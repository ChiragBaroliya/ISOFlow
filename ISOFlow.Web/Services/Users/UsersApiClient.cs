using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Users;

public interface IUsersApiClient
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(string id);
    Task<User?> CreateUserAsync(User user);
    Task<User?> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(string id);
    Task<List<User>> GetUsersByOrganizationIdAsync(string orgId);
    Task<bool> UpdateProfileAsync(string userId, string name, string phone, string department, string bio, string? avatarUrl = null);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}

public class UsersApiClient : IUsersApiClient
{
    private readonly IApiHttpClient _api;

    public UsersApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<User>> GetAllUsersAsync() =>
        await _api.GetAsync<List<User>>("api/users/all") ?? new();

    public async Task<User?> GetUserByIdAsync(string id) =>
        await _api.GetAsync<User>($"api/users/{Uri.EscapeDataString(id)}");

    public async Task<User?> CreateUserAsync(User user) =>
        await _api.PostAsync<User>("api/users", new
        {
            user.OrganizationId, user.Name, user.Email, user.Password,
            user.SystemRole, user.Role, user.Department, user.Location,
            user.Phone, user.Bio
        });

    public async Task<User?> UpdateUserAsync(User user) =>
        await _api.PutAsync<User>($"api/users/{Uri.EscapeDataString(user.Id)}", new
        {
            user.OrganizationId, user.Name, user.Email, user.Password,
            user.SystemRole, user.Role, user.Department, user.Location,
            user.Phone, user.Bio
        });

    public async Task<bool> DeleteUserAsync(string id) =>
        await _api.DeleteAsync($"api/users/{Uri.EscapeDataString(id)}");

    public async Task<List<User>> GetUsersByOrganizationIdAsync(string orgId)
    {
        var paged = await _api.GetAsync<PagedResponse<User>>(
            $"api/users/organization/{Uri.EscapeDataString(orgId)}?pageSize=100");
        return paged?.Items ?? new();
    }

    public async Task<bool> UpdateProfileAsync(string userId, string name, string phone, string department, string bio, string? avatarUrl = null) =>
        await _api.PutBoolAsync($"api/users/{Uri.EscapeDataString(userId)}/profile", new
        {
            Name = name, Phone = phone, Department = department, Bio = bio, AvatarUrl = avatarUrl
        });

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword) =>
        await _api.PostBoolAsync($"api/users/{Uri.EscapeDataString(userId)}/change-password", new
        {
            CurrentPassword = currentPassword, NewPassword = newPassword
        });
}
