using ISOFlow.Application.DTOs;

namespace ISOFlow.Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Validates user credentials, returns signed JWT access token and secure refresh token.
    /// </summary>
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Rotates refresh token: revokes provided token, verifies no reuse/expiry, and issues new token pair.
    /// </summary>
    Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);

    /// <summary>
    /// Revokes an active refresh token on user logout.
    /// </summary>
    Task<bool> RevokeTokenAsync(string refreshToken);

    /// <summary>
    /// Gets current authenticated user profile by user ID extracted from JWT claims.
    /// </summary>
    Task<UserProfileDto?> GetUserProfileByIdAsync(string userId);
}
