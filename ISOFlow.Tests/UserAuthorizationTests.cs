using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ISOFlow.Api.Controllers;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;
using ISOFlow.Infrastructure.Repositories;
using ISOFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ISOFlow.Tests;

/// <summary>
/// Regression coverage for the cross-user IDOR vulnerability: a valid access token for one user
/// must never be able to read or modify another user's account data, regardless of what id is
/// supplied in the route/body. Reproduces the reported scenario (Alex's token vs. Sarah's data)
/// plus the equivalent cross-organization and privilege-escalation variants.
/// </summary>
public class UserAuthorizationTests
{
    private const string AcmeOrg = "1";
    private const string OtherOrg = "2";

    private readonly MockUserRepository _userRepository = new();
    private readonly UsersController _controller;

    public UserAuthorizationTests()
    {
        var jwtSettings = new JwtSettings
        {
            Secret = "Test_Super_Secret_Key_At_Least_32_Bytes_Long_2026_Key",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15
        };
        var jwtService = new JwtService(jwtSettings);
        var dbFactory = new DbConnectionFactory("Host=localhost;Database=isoflow_mock;Username=postgres;Password=postgres;");
        var refreshTokenRepository = new RefreshTokenRepository(dbFactory);
        var authService = new AuthService(_userRepository, refreshTokenRepository, jwtService, jwtSettings);

        _controller = new UsersController(_userRepository, authService, jwtService);
    }

    private static void ActAs(UsersController controller, string userId, SystemRole role, string organizationId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role.ToString()),
            new("org_id", organizationId)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ── Reported scenario: Alex's token must never read or change Sarah's account ──────────────

    [Fact]
    public async Task GetById_PeerUserToken_CannotReadAnotherUsersProfile()
    {
        // Sarah (plain "User" role) attempts to read Alex's profile using her own valid token.
        ActAs(_controller, "sarah-id", SystemRole.User, AcmeOrg);

        var result = await _controller.GetById("alex-id");

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task UpdateProfile_AlexTokenAgainstSarahId_IsDenied()
    {
        // Alex is authenticated (any valid token) but puts Sarah's id in the route.
        ActAs(_controller, "alex-id", SystemRole.Admin, AcmeOrg);

        var result = await _controller.UpdateProfile("sarah-id", new UpdateProfileDto { Name = "Hacked Name" });

        Assert.IsType<ForbidResult>(result.Result);
        Assert.Equal("Sarah Chen", _userRepository.Get("sarah-id")!.Name); // untouched
    }

    [Fact]
    public async Task ChangePassword_AlexTokenAgainstSarahId_IsDenied()
    {
        ActAs(_controller, "alex-id", SystemRole.Admin, AcmeOrg);

        var result = await _controller.ChangePassword("sarah-id", new ChangePasswordDto
        {
            CurrentPassword = "Test@123",
            NewPassword = "PwnedPassword1"
        });

        Assert.IsType<ForbidResult>(result.Result);
        Assert.Equal("Test@123", _userRepository.Get("sarah-id")!.Password); // untouched
    }

    [Fact]
    public async Task UpdateProfile_OwnId_Succeeds()
    {
        ActAs(_controller, "sarah-id", SystemRole.User, AcmeOrg);

        var result = await _controller.UpdateProfile("sarah-id", new UpdateProfileDto { Name = "Sarah C. Chen" });

        Assert.IsType<OkObjectResult>(result.Result);
    }

    // ── Route/body id manipulation must not bypass identity derived from the token ─────────────

    [Theory]
    [InlineData("sarah-id")]
    [InlineData("does-not-exist")]
    [InlineData("' OR '1'='1")]
    public async Task ChangePassword_AnyNonSelfIdInRoute_IsDenied(string spoofedId)
    {
        ActAs(_controller, "alex-id", SystemRole.Admin, AcmeOrg);

        var result = await _controller.ChangePassword(spoofedId, new ChangePasswordDto
        {
            CurrentPassword = "Test@123",
            NewPassword = "NewPass123"
        });

        Assert.IsType<ForbidResult>(result.Result);
    }

    // ── Admin management (legitimate) vs. cross-organization access (must be denied) ───────────

    [Fact]
    public async Task GetById_AdminSameOrganization_CanViewTeamMember()
    {
        // Legitimate case: an org Admin managing a user within their own organization.
        ActAs(_controller, "alex-id", SystemRole.Admin, AcmeOrg);

        var result = await _controller.GetById("sarah-id");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<UserProfileDto>>(ok.Value);
        Assert.Equal("sarah-id", response.Data!.Id);
        Assert.Null(response.Data.GetType().GetProperty("Password")); // DTO never exposes a password field
    }

    [Fact]
    public async Task GetById_AdminFromDifferentOrganization_IsDenied()
    {
        ActAs(_controller, "dana-id", SystemRole.Admin, OtherOrg);

        var result = await _controller.GetById("sarah-id");

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task GetByOrg_DifferentOrganization_IsDenied()
    {
        ActAs(_controller, "alex-id", SystemRole.Admin, AcmeOrg);

        var result = await _controller.GetByOrg(OtherOrg, new PagedRequestDto());

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_NonSuperAdmin_OnlySeesOwnOrganization()
    {
        ActAs(_controller, "alex-id", SystemRole.Admin, AcmeOrg);

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<UserProfileDto>>>(ok.Value);
        Assert.All(response.Data!, u => Assert.Equal(AcmeOrg, u.OrganizationId));
        Assert.DoesNotContain(response.Data!, u => u.Id == "dana-id");
    }

    [Fact]
    public async Task GetAll_SuperAdmin_SeesAllOrganizations()
    {
        ActAs(_controller, "root-id", SystemRole.SuperAdmin, string.Empty);

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<UserProfileDto>>>(ok.Value);
        Assert.Contains(response.Data!, u => u.Id == "dana-id");
    }

    // ── Privilege escalation guard on the write endpoints ───────────────────────────────────────

    [Fact]
    public async Task Update_AdminAttemptsToPromoteUserToSuperAdmin_IsDenied()
    {
        ActAs(_controller, "alex-id", SystemRole.Admin, AcmeOrg);

        var result = await _controller.Update("sarah-id", new UserRequestDto
        {
            Name = "Sarah Chen",
            Email = "sarah.chen@acme.com",
            SystemRole = SystemRole.SuperAdmin,
            Role = "Compliance Manager",
            OrganizationId = AcmeOrg
        });

        Assert.IsType<ForbidResult>(result.Result);
        Assert.Equal(SystemRole.User, _userRepository.Get("sarah-id")!.SystemRole); // untouched
    }

    [Fact]
    public async Task Update_AdminAttemptsCrossOrganizationEdit_IsDenied()
    {
        ActAs(_controller, "dana-id", SystemRole.Admin, OtherOrg);

        var result = await _controller.Update("sarah-id", new UserRequestDto
        {
            Name = "Renamed By Outsider",
            Email = "sarah.chen@acme.com",
            SystemRole = SystemRole.User,
            Role = "Compliance Manager",
            OrganizationId = AcmeOrg
        });

        Assert.IsType<ForbidResult>(result.Result);
        Assert.Equal("Sarah Chen", _userRepository.Get("sarah-id")!.Name); // untouched
    }

    [Fact]
    public async Task Delete_AdminAttemptsCrossOrganizationDelete_IsDenied()
    {
        ActAs(_controller, "dana-id", SystemRole.Admin, OtherOrg);

        var result = await _controller.Delete("sarah-id");

        Assert.IsType<ForbidResult>(result.Result);
        Assert.NotNull(_userRepository.Get("sarah-id")); // untouched
    }

    // ── Minimal in-memory repository double, seeded with the reported scenario's users ─────────

    private sealed class MockUserRepository : IUserRepository
    {
        private readonly List<User> _users = new()
        {
            new User { Id = "alex-id", Name = "Alex Morgan", Email = "alex.morgan@acme.com", Password = "Test@123", SystemRole = SystemRole.Admin, Role = "Compliance Lead", Department = "Compliance", OrganizationId = int.Parse(AcmeOrg) },
            new User { Id = "sarah-id", Name = "Sarah Chen", Email = "sarah.chen@acme.com", Password = "Test@123", SystemRole = SystemRole.User, Role = "Compliance Manager", Department = "Compliance", OrganizationId = int.Parse(AcmeOrg) },
            new User { Id = "dana-id", Name = "Dana Cross", Email = "dana@globex.com", Password = "Test@123", SystemRole = SystemRole.Admin, Role = "IT Lead", Department = "IT", OrganizationId = int.Parse(OtherOrg) },
            new User { Id = "root-id", Name = "Root SuperAdmin", Email = "root@isoflow.io", Password = "Test@123", SystemRole = SystemRole.SuperAdmin, Role = "Platform Admin", Department = "Platform", OrganizationId = null }
        };

        public User? Get(string id) => _users.FirstOrDefault(u => u.Id == id);

        public Task<List<User>> GetAllUsersAsync() => Task.FromResult(_users.ToList());
        public Task<PagedResponse<User>> GetPagedUsersAsync(PagedRequestDto request) => Task.FromResult(new PagedResponse<User>(_users.ToList(), _users.Count, 1, 50));
        public Task<List<User>> GetUsersByOrganizationIdAsync(int organizationId) => Task.FromResult(_users.Where(u => u.OrganizationId == organizationId).ToList());
        public Task<PagedResponse<User>> GetPagedUsersByOrganizationIdAsync(int organizationId, PagedRequestDto request)
        {
            var items = _users.Where(u => u.OrganizationId == organizationId).ToList();
            return Task.FromResult(new PagedResponse<User>(items, items.Count, 1, 50));
        }
        public Task<User?> GetUserByIdAsync(string id) => Task.FromResult(Get(id));
        public Task<User?> GetUserByEmailAsync(string email) => Task.FromResult(_users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        public Task<User> CreateUserAsync(User user) { user.Id = Guid.NewGuid().ToString(); _users.Add(user); return Task.FromResult(user); }
        public Task<User?> UpdateUserAsync(User user) { var idx = _users.FindIndex(u => u.Id == user.Id); if (idx >= 0) _users[idx] = user; return Task.FromResult<User?>(user); }
        public Task<bool> DeleteUserAsync(string id) => Task.FromResult(_users.RemoveAll(u => u.Id == id) > 0);
        public Task<User?> ValidateLoginAsync(string email, string password)
        {
            var u = _users.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(u != null && u.Password == password ? u : null);
        }
        public Task<string?> GenerateResetTokenAsync(string email) => Task.FromResult<string?>("RESET123");
        public Task<bool> ResetPasswordAsync(string token, string newPassword) => Task.FromResult(true);

        public Task<bool> UpdateProfileAsync(string id, string name, string phone, string department, string bio)
        {
            var user = Get(id);
            if (user == null) return Task.FromResult(false);
            user.Name = name;
            user.Phone = phone;
            user.Department = department;
            user.Bio = bio;
            return Task.FromResult(true);
        }

        public Task<bool> ChangePasswordAsync(string id, string currentPassword, string newPassword)
        {
            var user = Get(id);
            if (user == null || user.Password != currentPassword) return Task.FromResult(false);
            user.Password = newPassword;
            return Task.FromResult(true);
        }
    }
}
