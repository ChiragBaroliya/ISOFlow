using ISOFlow.Application.DTOs;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Auth;

public interface IAuthApiClient
{
    Task<LoginResponseDto?> LoginAsync(string email, string password);
    /// <summary>Requests a reset link by email. The API never returns the token.</summary>
    Task ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    Task<bool> LogoutAsync(string refreshToken);
}

public class AuthApiClient : IAuthApiClient
{
    private readonly IApiHttpClient _api;

    public AuthApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<LoginResponseDto?> LoginAsync(string email, string password) =>
        await _api.PostAsync<LoginResponseDto>("api/auth/login", new { Email = email, Password = password });

    public async Task ForgotPasswordAsync(string email) =>
        await _api.PostAsync<string>("api/users/forgot-password", new { Email = email });

    public async Task<bool> ResetPasswordAsync(string token, string newPassword) =>
        await _api.PostBoolAsync("api/users/reset-password", new { Token = token, NewPassword = newPassword });

    public async Task<bool> LogoutAsync(string refreshToken) =>
        await _api.PostBoolAsync("api/auth/logout", new { RefreshToken = refreshToken });
}
