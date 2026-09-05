using System.Security.Claims;
using ISOFlow.Api.Auditing;
using ISOFlow.Api.Extensions;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// User Management, Roles and Authentication API
/// </summary>
[Authorize]
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
    /// Get paginated and filtered list of Users. Non-SuperAdmin callers only ever see their own organization.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserProfileDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<ActionResult<ApiResponse<PagedResponse<UserProfileDto>>>> GetPaged([FromQuery] PagedRequestDto request)
    {
        var paged = User.IsSuperAdmin()
            ? await _userRepository.GetPagedUsersAsync(request)
            : await _userRepository.GetPagedUsersByOrganizationIdAsync(User.GetOrganizationIdOrNull() ?? 0, request);

        var profiles = new PagedResponse<UserProfileDto>(paged.Items.Select(MapToProfile).ToList(), paged.TotalCount, paged.PageNumber, paged.PageSize);
        return Ok(ApiResponse<PagedResponse<UserProfileDto>>.SuccessResponse(profiles));
    }

    /// <summary>
    /// Get all registered Users. Non-SuperAdmin callers only ever see their own organization.
    /// </summary>
    [HttpGet("all")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<UserProfileDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<ActionResult<ApiResponse<List<UserProfileDto>>>> GetAll()
    {
        var users = User.IsSuperAdmin()
            ? await _userRepository.GetAllUsersAsync()
            : await _userRepository.GetUsersByOrganizationIdAsync(User.GetOrganizationIdOrNull() ?? 0);

        return Ok(ApiResponse<List<UserProfileDto>>.SuccessResponse(users.Select(MapToProfile).ToList()));
    }

    /// <summary>
    /// Get Users belonging to an Organization. Callers may only request their own organization unless they are SuperAdmin.
    /// </summary>
    [HttpGet("organization/{orgId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserProfileDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<ActionResult<ApiResponse<PagedResponse<UserProfileDto>>>> GetByOrg(string orgId, [FromQuery] PagedRequestDto request)
    {
        if (!User.IsSuperAdmin() && !User.IsSameOrganization(orgId))
            return Forbid();

        if (!int.TryParse(orgId, out var orgIdInt))
            return NotFound(ApiResponse<PagedResponse<UserProfileDto>>.FailureResponse($"Organization '{orgId}' was not found."));

        var paged = await _userRepository.GetPagedUsersByOrganizationIdAsync(orgIdInt, request);
        var profiles = new PagedResponse<UserProfileDto>(paged.Items.Select(MapToProfile).ToList(), paged.TotalCount, paged.PageNumber, paged.PageSize);
        return Ok(ApiResponse<PagedResponse<UserProfileDto>>.SuccessResponse(profiles));
    }

    /// <summary>
    /// Get User details by Identifier. Callers may only view their own record, or (if Admin/SuperAdmin)
    /// a record belonging to their own organization; SuperAdmin may view any organization.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), 404)]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetById(string id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<UserProfileDto>.FailureResponse($"User with ID '{id}' was not found."));

        var isPrivilegedSameOrgAdmin = User.IsAdmin() && User.IsSameOrganization(user.OrganizationId?.ToString());
        if (!User.IsSelf(id) && !User.IsSuperAdmin() && !isPrivilegedSameOrgAdmin)
            return Forbid();

        return Ok(ApiResponse<UserProfileDto>.SuccessResponse(MapToProfile(user)));
    }

    /// <summary>
    /// Register a new User (Requires SuperAdmin or Admin role)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [Audit(Module = "Users", Entity = "User", Action = AuditActionType.Create)]
    [ProducesResponseType(typeof(ApiResponse<User>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<ActionResult<ApiResponse<User>>> Create([FromBody] UserRequestDto dto)
    {
        if (!User.IsSuperAdmin())
        {
            // Admins may only provision users within their own organization, and can never mint a SuperAdmin.
            if (dto.SystemRole == SystemRole.SuperAdmin)
                return Forbid();
            dto.OrganizationId = User.GetOrganizationId() ?? string.Empty;
        }

        var user = new User
        {
            OrganizationId = string.IsNullOrWhiteSpace(dto.OrganizationId) ? null : int.Parse(dto.OrganizationId),
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
    [Audit(Module = "Users", Entity = "User", Action = AuditActionType.Update)]
    [ProducesResponseType(typeof(ApiResponse<User>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<User>), 404)]
    public async Task<ActionResult<ApiResponse<User>>> Update(string id, [FromBody] UserRequestDto dto)
    {
        var existing = await _userRepository.GetUserByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse<User>.FailureResponse($"User with ID '{id}' was not found."));

        if (!User.IsSuperAdmin())
        {
            // Admins may only manage users within their own organization, and can never touch/create a SuperAdmin.
            if (!User.IsSameOrganization(existing.OrganizationId?.ToString()) ||
                existing.SystemRole == SystemRole.SuperAdmin ||
                dto.SystemRole == SystemRole.SuperAdmin)
                return Forbid();

            dto.OrganizationId = existing.OrganizationId?.ToString();
        }

        existing.Name = dto.Name;
        existing.Email = dto.Email;
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            existing.Password = dto.Password;
        }
        existing.SystemRole = dto.SystemRole;
        existing.Role = dto.Role;
        existing.Department = dto.Department;
        existing.OrganizationId = string.IsNullOrWhiteSpace(dto.OrganizationId) ? null : int.Parse(dto.OrganizationId);
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
    [Audit(Module = "Users", Entity = "User", Action = AuditActionType.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        if (!User.IsSuperAdmin())
        {
            var existing = await _userRepository.GetUserByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<bool>.FailureResponse($"User with ID '{id}' was not found."));

            // Admins may only remove users within their own organization, and can never remove a SuperAdmin.
            if (!User.IsSameOrganization(existing.OrganizationId?.ToString()) || existing.SystemRole == SystemRole.SuperAdmin)
                return Forbid();
        }

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
    /// Update User Profile details. Self-service only: a caller may only update their own profile,
    /// regardless of which id is supplied in the route.
    /// </summary>
    [HttpPut("{id}/profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateProfile(string id, [FromBody] UpdateProfileDto dto)
    {
        if (!User.IsSelf(id))
            return Forbid();

        var success = await _userRepository.UpdateProfileAsync(id, dto.Name, dto.Phone, dto.Department, dto.Bio, dto.AvatarUrl);
        if (!success)
            return NotFound(ApiResponse<bool>.FailureResponse($"User with ID '{id}' was not found."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Profile updated successfully."));
    }

    /// <summary>
    /// Change password with current password verification. Self-service only: a caller may only change
    /// their own password, regardless of which id is supplied in the route.
    /// </summary>
    [HttpPost("{id}/change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(string id, [FromBody] ChangePasswordDto dto)
    {
        if (!User.IsSelf(id))
            return Forbid();

        var success = await _userRepository.ChangePasswordAsync(id, dto.CurrentPassword, dto.NewPassword);
        if (!success)
            return BadRequest(ApiResponse<bool>.FailureResponse("Current password does not match."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Password changed successfully."));
    }

    private static UserProfileDto MapToProfile(User user) => new()
    {
        Id = user.Id,
        OrganizationId = user.OrganizationId?.ToString() ?? string.Empty,
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
