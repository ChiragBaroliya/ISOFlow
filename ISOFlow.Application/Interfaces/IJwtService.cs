using System.Security.Claims;
using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IJwtService
{
    /// <summary>
    /// Generates a cryptographically signed JWT access token containing standard and role claims.
    /// </summary>
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);

    /// <summary>
    /// Generates a cryptographically secure random refresh token string and computes its SHA-256 hash.
    /// </summary>
    (string PlainToken, string TokenHash, DateTime ExpiresAt) GenerateRefreshToken();

    /// <summary>
    /// Computes SHA-256 hash of a plain refresh token for database lookup.
    /// </summary>
    string HashRefreshToken(string plainToken);

    /// <summary>
    /// Validates an access token and returns ClaimsPrincipal if valid; otherwise null.
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Extracts User ID claim ('sub' or ClaimTypes.NameIdentifier) from ClaimsPrincipal.
    /// </summary>
    string? GetUserIdFromPrincipal(ClaimsPrincipal principal);
}
