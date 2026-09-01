using System.Security.Claims;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// User Management, Roles and Authentication API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;

    public UsersController(
        IUserRepository userRepository,
        IAuthService authService,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _authService = authService;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Get the profile and claims of the currently authenticated user
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
            return Unauthorized(ApiResponse<UserProfileDto>.FailureResponse("User identity could not be established from JWT claims."));

        var profile = await _authService.GetUserProfileByIdAsync(userId);
        if (profile == null)
            return NotFound(ApiResponse<UserProfileDto>.FailureResponse($"User with ID '{userId}' was not found."));

        return Ok(ApiResponse<UserProfileDto>.SuccessResponse(profile));
    }

    /// <summary>
    /// Get paginated and filtered list of Users
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<User>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<ActionResult<ApiResponse<PagedResponse<User>>>> GetPaged([FromQuery] PagedRequestDto request)
    {
        var paged = await _userRepository.GetPagedUsersAsync(request);
        return Ok(ApiResponse<PagedResponse<User>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all registered Users
    /// </summary>
    [HttpGet("all")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<User>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<ActionResult<ApiResponse<List<User>>>> GetAll()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return Ok(ApiResponse<List<User>>.SuccessResponse(users));
    }

    /// <summary>
    /// Get Users belonging to an Organization
    /// </summary>
    [HttpGet("organization/{orgId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<User>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<ActionResult<ApiResponse<PagedResponse<User>>>> GetByOrg(string orgId, [FromQuery] PagedRequestDto request)
    {
        var paged = await _userRepository.GetPagedUsersByOrganizationIdAsync(orgId, request);
        return Ok(ApiResponse<PagedResponse<User>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get User details by Identifier
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<User>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<User>), 404)]
    public async Task<ActionResult<ApiResponse<User>>> GetById(string id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<User>.FailureResponse($"User with ID '{id}' was not found."));

        return Ok(ApiResponse<User>.SuccessResponse(user));
    }

    /// <summary>
    /// Register a new User (Requires SuperAdmin or Admin role)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<User>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<ActionResult<ApiResponse<User>>> Create([FromBody] UserRequestDto dto)
    {
        var user = new User
        {
            OrganizationId = dto.OrganizationId ?? string.Empty,
            Name = dto.Name,
            Email = dto.Email,
            Password = dto.Password,
            SystemRole = dto.SystemRole,
            Role = dto.Role,
            Department = dto.Department,
            Location = dto.Location,
            Phone = dto.Phone,
            Bio = dto.Bio
        };

        var created = await _userRepository.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<User>.SuccessResponse(created, "User registered successfully."));
    }

    /// <summary>
    /// Update existing User (Requires SuperAdmin or Admin role)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<User>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<User>), 404)]
    public async Task<ActionResult<ApiResponse<User>>> Update(string id, [FromBody] UserRequestDto dto)
    {
        var existing = await _userRepository.GetUserByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse<User>.FailureResponse($"User with ID '{id}' was not found."));

        existing.Name = dto.Name;
        existing.Email = dto.Email;
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            existing.Password = dto.Password;
        }
        existing.SystemRole = dto.SystemRole;
        existing.Role = dto.Role;
        existing.Department = dto.Department;
        existing.OrganizationId = dto.OrganizationId ?? string.Empty;
        existing.Location = dto.Location;
        existing.Phone = dto.Phone;
        existing.Bio = dto.Bio;

        var updated = await _userRepository.UpdateUserAsync(existing);
        return Ok(ApiResponse<User>.SuccessResponse(updated!, "User updated successfully."));
    }

    /// <summary>
    /// Delete a User (Requires SuperAdmin or Admin role)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        var deleted = await _userRepository.DeleteUserAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailureResponse($"User with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "User deleted successfully."));
    }

    /// <summary>
    /// Authenticate User credentials
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] UserLoginDto dto)
    {
        var loginResponse = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = dto.Email,
            Password = dto.Password
        });

        if (loginResponse == null)
            return Unauthorized(ApiResponse<LoginResponseDto>.FailureResponse("Invalid email or password."));

        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(loginResponse, "Login successful."));
    }

    /// <summary>
    /// Request password reset token
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var token = await _userRepository.GenerateResetTokenAsync(dto.Email);
        if (token == null)
            return NotFound(ApiResponse<string>.FailureResponse($"No account found with email '{dto.Email}'."));

        return Ok(ApiResponse<string>.SuccessResponse(token, "Reset token generated successfully."));
    }

    /// <summary>
    /// Reset password using reset token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var success = await _userRepository.ResetPasswordAsync(dto.Token, dto.NewPassword);
        if (!success)
            return BadRequest(ApiResponse<bool>.FailureResponse("Invalid or expired reset token."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Password has been reset successfully."));
    }

    /// <summary>
    /// Update User Profile details
    /// </summary>
    [HttpPut("{id}/profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateProfile(string id, [FromBody] UpdateProfileDto dto)
    {
        var success = await _userRepository.UpdateProfileAsync(id, dto.Name, dto.Phone, dto.Department, dto.Bio);
        if (!success)
            return NotFound(ApiResponse<bool>.FailureResponse($"User with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Profile updated successfully."));
    }

    /// <summary>
    /// Change password with current password verification
    /// </summary>
    [HttpPost("{id}/change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(string id, [FromBody] ChangePasswordDto dto)
    {
        var success = await _userRepository.ChangePasswordAsync(id, dto.CurrentPassword, dto.NewPassword);
        if (!success)
            return BadRequest(ApiResponse<bool>.FailureResponse("Current password does not match."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Password changed successfully."));
    }
}
