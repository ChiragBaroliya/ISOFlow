using System.Security.Claims;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// Authentication and Token Management API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;

    public AuthController(IAuthService authService, IJwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Authenticate user credentials and generate JWT Access and Refresh Tokens
    /// </summary>
    /// <remarks>
    /// Validates user email and password against stored PBKDF2 hashes. Returns a signed JWT Bearer access token (15-minute default validity) and a refresh token (7-day validity).
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        if (response == null)
        {
            return Unauthorized(ApiResponse<LoginResponseDto>.FailureResponse("Invalid email or password."));
        }

        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Authentication successful."));
    }

    /// <summary>
    /// Refresh access token using a valid Refresh Token
    /// </summary>
    /// <remarks>
    /// Validates the refresh token hash, invalidates the used token, checks against replay/reuse attacks, and issues a new access token and rotated refresh token.
    /// </remarks>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        if (response == null)
        {
            return Unauthorized(ApiResponse<TokenResponseDto>.FailureResponse("Invalid, expired, or revoked refresh token."));
        }

        return Ok(ApiResponse<TokenResponseDto>.SuccessResponse(response, "Token refreshed successfully."));
    }

    /// <summary>
    /// Revoke active Refresh Token on User Logout
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] RevokeTokenRequestDto request)
    {
        var result = await _authService.RevokeTokenAsync(request.RefreshToken);
        return Ok(ApiResponse<bool>.SuccessResponse(result, result ? "Logged out and token revoked successfully." : "Token was not found or already revoked."));
    }

    /// <summary>
    /// Get the profile and identity claims of the currently authenticated user
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetCurrentUser()
    {
        var userId = _jwtService.GetUserIdFromPrincipal(User)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<UserProfileDto>.FailureResponse("User identity could not be established from JWT claims."));
        }

        var profile = await _authService.GetUserProfileByIdAsync(userId);
        if (profile == null)
        {
            return NotFound(ApiResponse<UserProfileDto>.FailureResponse($"User profile for ID '{userId}' was not found."));
        }

        return Ok(ApiResponse<UserProfileDto>.SuccessResponse(profile));
    }
}
