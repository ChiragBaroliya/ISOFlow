using System.ComponentModel.DataAnnotations;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Application.DTOs;

/// <summary>
/// Strongly-typed JWT and Token Expiration settings
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = "ISOFlow_Super_Secure_JWT_Secret_Key_At_Least_32_Bytes_2026";
    public string Issuer { get; set; } = "ISOFlowApi";
    public string Audience { get; set; } = "ISOFlowClient";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

/// <summary>
/// Request payload for user login
/// </summary>
public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Token pair and metadata returned upon successful authentication
/// </summary>
public class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresInSeconds { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}

/// <summary>
/// Full login response containing tokens and user identity summary
/// </summary>
public class LoginResponseDto
{
    public TokenResponseDto Tokens { get; set; } = new();
    public UserProfileDto User { get; set; } = new();
}

/// <summary>
/// User identity summary safe for frontend exposure (excludes password/hashes)
/// </summary>
public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public SystemRole SystemRole { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}

/// <summary>
/// Request payload to rotate an expired access token using a valid refresh token
/// </summary>
public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Refresh Token is required.")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Request payload to revoke a refresh token on logout
/// </summary>
public class RevokeTokenRequestDto
{
    [Required(ErrorMessage = "Refresh Token is required.")]
    public string RefreshToken { get; set; } = string.Empty;
}
