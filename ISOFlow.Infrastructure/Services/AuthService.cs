using ISOFlow.Application.DTOs;
using ISOFlow.Application.Helpers;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ISOFlow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService>? _logger;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        JwtSettings jwtSettings,
        ILogger<AuthService>? logger = null)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
        _logger = logger;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return null;

        var user = await _userRepository.GetUserByEmailAsync(request.Email.Trim());
        if (user == null)
        {
            _logger?.LogWarning("Authentication failed: User with email '{Email}' was not found.", request.Email);
            return null;
        }

        var isPasswordValid = PasswordSecurityHelper.VerifyPassword(request.Password, user.Password);
        if (!isPasswordValid)
        {
            _logger?.LogWarning("Authentication failed: Invalid credentials provided for '{Email}'.", request.Email);
            return null;
        }

        // Generate Access Token & Refresh Token pair
        var (accessToken, accessExpiresAt) = _jwtService.GenerateAccessToken(user);
        var (plainRefreshToken, refreshTokenHash, refreshExpiresAt) = _jwtService.GenerateRefreshToken();

        // Save hashed refresh token to persistent store
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshExpiresAt
        };
        await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

        _logger?.LogInformation("User '{UserId}' ({Email}) authenticated successfully with role '{Role}'.", user.Id, user.Email, user.SystemRole);

        return new LoginResponseDto
        {
            Tokens = new TokenResponseDto
            {
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresInSeconds = (int)(accessExpiresAt - DateTime.UtcNow).TotalSeconds,
                ExpiresAt = accessExpiresAt,
                RefreshToken = plainRefreshToken,
                RefreshTokenExpiresAt = refreshExpiresAt
            },
            User = MapToUserProfile(user)
        };
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            return null;

        var tokenHash = _jwtService.HashRefreshToken(request.RefreshToken.Trim());
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken == null)
        {
            _logger?.LogWarning("Refresh token rotation rejected: Token was not found.");
            return null;
        }

        // Detect token reuse / replay attack: If a revoked token is used again, invalidate ALL tokens for this user
        if (storedToken.IsRevoked)
        {
            _logger?.LogWarning("Security Alert: Attempted reuse of revoked refresh token by User '{UserId}'. Revoking all tokens.", storedToken.UserId);
            await _refreshTokenRepository.RevokeAllForUserAsync(storedToken.UserId);
            return null;
        }

        // Check if token has expired
        if (storedToken.IsExpired)
        {
            _logger?.LogWarning("Refresh token rotation rejected: Token for User '{UserId}' has expired.", storedToken.UserId);
            return null;
        }

        var user = await _userRepository.GetUserByIdAsync(storedToken.UserId);
        if (user == null)
        {
            _logger?.LogWarning("Refresh token rotation rejected: User '{UserId}' no longer exists.", storedToken.UserId);
            return null;
        }

        // Generate new token pair
        var (newAccessToken, accessExpiresAt) = _jwtService.GenerateAccessToken(user);
        var (newPlainRefreshToken, newRefreshTokenHash, refreshExpiresAt) = _jwtService.GenerateRefreshToken();

        // Mark old token as revoked and replaced
        await _refreshTokenRepository.RevokeByTokenHashAsync(tokenHash, newRefreshTokenHash);

        // Store new active refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshExpiresAt
        };
        await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity);

        _logger?.LogInformation("Successfully rotated tokens for User '{UserId}'.", user.Id);

        return new TokenResponseDto
        {
            AccessToken = newAccessToken,
            TokenType = "Bearer",
            ExpiresInSeconds = (int)(accessExpiresAt - DateTime.UtcNow).TotalSeconds,
            ExpiresAt = accessExpiresAt,
            RefreshToken = newPlainRefreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return false;

        var tokenHash = _jwtService.HashRefreshToken(refreshToken.Trim());
        var success = await _refreshTokenRepository.RevokeByTokenHashAsync(tokenHash);

        if (success)
        {
            _logger?.LogInformation("Refresh token revoked successfully.");
        }

        return success;
    }

    public async Task<UserProfileDto?> GetUserProfileByIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var user = await _userRepository.GetUserByIdAsync(userId);
        return user != null ? MapToUserProfile(user) : null;
    }

    private static UserProfileDto MapToUserProfile(User user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            OrganizationId = user.OrganizationId,
            Name = user.Name,
            Email = user.Email,
            SystemRole = user.SystemRole,
            Role = user.Role,
            Department = user.Department,
            Location = user.Location,
            AvatarUrl = user.AvatarUrl,
            Phone = user.Phone,
            Bio = user.Bio
        };
    }
}
