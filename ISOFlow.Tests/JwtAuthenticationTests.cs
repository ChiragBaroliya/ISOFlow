using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ISOFlow.Api.Controllers;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Helpers;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;
using ISOFlow.Infrastructure.Repositories;
using ISOFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace ISOFlow.Tests;

public class JwtAuthenticationTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtService _jwtService;
    private readonly MockUserRepository _userRepository;
    private readonly RefreshTokenRepository _refreshTokenRepository;
    private readonly AuthService _authService;

    public JwtAuthenticationTests()
    {
        _jwtSettings = new JwtSettings
        {
            Secret = "Test_Super_Secret_Key_At_Least_32_Bytes_Long_2026_Key",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        _jwtService = new JwtService(_jwtSettings);
        _userRepository = new MockUserRepository();
        var dbFactory = new DbConnectionFactory("Host=localhost;Database=isoflow_mock;Username=postgres;Password=postgres;");
        _refreshTokenRepository = new RefreshTokenRepository(dbFactory);
        _authService = new AuthService(_userRepository, _refreshTokenRepository, _jwtService, _jwtSettings);
    }

    #region PasswordSecurityHelper Tests

    [Fact]
    public void PasswordSecurityHelper_HashAndVerify_Success()
    {
        var password = "SecurePassword@2026";
        var hash = PasswordSecurityHelper.HashPassword(password);

        Assert.StartsWith("$pbkdf2-sha256$", hash);
        Assert.True(PasswordSecurityHelper.VerifyPassword(password, hash));
        Assert.False(PasswordSecurityHelper.VerifyPassword("WrongPassword", hash));
    }

    [Fact]
    public void PasswordSecurityHelper_VerifyLegacyPlainTextPassword_Success()
    {
        var legacyPassword = "Test@123";
        Assert.True(PasswordSecurityHelper.VerifyPassword("Test@123", legacyPassword));
        Assert.False(PasswordSecurityHelper.VerifyPassword("WrongPass", legacyPassword));
    }

    [Fact]
    public void PasswordSecurityHelper_GenerateResetToken_ValidFormat()
    {
        var token = PasswordSecurityHelper.GenerateResetToken(10);
        Assert.Equal(10, token.Length);
        Assert.Matches("^[A-Z0-9]{10}$", token);
    }

    #endregion

    #region JwtService Tests

    [Fact]
    public void JwtService_GenerateAccessToken_ContainsExpectedClaimsAndMetadata()
    {
        var user = new User
        {
            Id = "101",
            Name = "Alice Admin",
            Email = "alice@acme.com",
            SystemRole = SystemRole.Admin,
            Role = "Compliance Lead",
            OrganizationId = "5"
        };

        var (token, expiresAt) = _jwtService.GenerateAccessToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiresAt > DateTime.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal(_jwtSettings.Issuer, jwtToken.Issuer);
        Assert.Contains(_jwtSettings.Audience, jwtToken.Audiences);
        Assert.Equal("101", jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("alice@acme.com", jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("Alice Admin", jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value);
        Assert.Equal("Admin", jwtToken.Claims.First(c => c.Type == "role" || c.Type == ClaimTypes.Role).Value);
        Assert.Equal("5", jwtToken.Claims.First(c => c.Type == "org_id").Value);
        Assert.Equal("Compliance Lead", jwtToken.Claims.First(c => c.Type == "job_role").Value);
        Assert.NotNull(jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti));
    }

    [Fact]
    public void JwtService_ValidateToken_ValidTokenReturnsPrincipal()
    {
        var user = new User
        {
            Id = "202",
            Name = "Bob SuperAdmin",
            Email = "bob@acme.com",
            SystemRole = SystemRole.SuperAdmin
        };

        var (token, _) = _jwtService.GenerateAccessToken(user);
        var principal = _jwtService.ValidateToken(token);

        Assert.NotNull(principal);
        var userId = _jwtService.GetUserIdFromPrincipal(principal!);
        Assert.Equal("202", userId);
    }

    [Fact]
    public void JwtService_ValidateToken_TamperedSignatureReturnsNull()
    {
        var user = new User { Id = "303", Email = "charlie@acme.com", SystemRole = SystemRole.User };
        var (token, _) = _jwtService.GenerateAccessToken(user);

        // Tamper with the payload part of the JWT
        var parts = token.Split('.');
        var tamperedToken = $"{parts[0]}.eyJJbnZhbGlkIjoiVHJ1ZSJ9.{parts[2]}";

        var principal = _jwtService.ValidateToken(tamperedToken);
        Assert.Null(principal);
    }

    [Fact]
    public void JwtService_ValidateToken_ExpiredTokenReturnsNull()
    {
        var expiredSettings = new JwtSettings
        {
            Secret = _jwtSettings.Secret,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            AccessTokenExpirationMinutes = -5 // already expired
        };
        var expiredService = new JwtService(expiredSettings);

        var user = new User { Id = "404", Email = "expired@acme.com", SystemRole = SystemRole.User };
        var (expiredToken, _) = expiredService.GenerateAccessToken(user);

        var principal = _jwtService.ValidateToken(expiredToken);
        Assert.Null(principal);
    }

    [Fact]
    public void JwtService_ValidateToken_InvalidIssuerOrAudienceReturnsNull()
    {
        var otherSettings = new JwtSettings
        {
            Secret = _jwtSettings.Secret,
            Issuer = "ForeignIssuer",
            Audience = "ForeignAudience",
            AccessTokenExpirationMinutes = 15
        };
        var foreignService = new JwtService(otherSettings);

        var user = new User { Id = "505", Email = "foreign@acme.com", SystemRole = SystemRole.User };
        var (foreignToken, _) = foreignService.GenerateAccessToken(user);

        // Validating with _jwtService (configured for TestIssuer/TestAudience) must fail
        var principal = _jwtService.ValidateToken(foreignToken);
        Assert.Null(principal);
    }

    [Fact]
    public void JwtService_RefreshTokenHashing_DeterministicAndSecure()
    {
        var (plainToken, hash, expiresAt) = _jwtService.GenerateRefreshToken();

        Assert.False(string.IsNullOrWhiteSpace(plainToken));
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.True(expiresAt > DateTime.UtcNow);

        var computedHash = _jwtService.HashRefreshToken(plainToken);
        Assert.Equal(hash, computedHash);
    }

    #endregion

    #region AuthService & Refresh Token Flow Tests

    [Fact]
    public async Task AuthService_Login_ValidCredentials_ReturnsTokensAndUserProfile()
    {
        var response = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = "admin@isoflow.io",
            Password = "Test@123"
        });

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response!.Tokens.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(response.Tokens.RefreshToken));
        Assert.Equal("Bearer", response.Tokens.TokenType);
        Assert.Equal("admin@isoflow.io", response.User.Email);
        Assert.Equal(SystemRole.SuperAdmin, response.User.SystemRole);
    }

    [Fact]
    public async Task AuthService_Login_InvalidPassword_ReturnsNull()
    {
        var response = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = "admin@isoflow.io",
            Password = "WrongPassword"
        });

        Assert.Null(response);
    }

    [Fact]
    public async Task AuthService_Login_NonExistentEmail_ReturnsNull()
    {
        var response = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = "unknown@isoflow.io",
            Password = "Test@123"
        });

        Assert.Null(response);
    }

    [Fact]
    public async Task AuthService_RefreshTokenRotation_Success()
    {
        // 1. Initial Login
        var login = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = "user@isoflow.io",
            Password = "Test@123"
        });
        Assert.NotNull(login);

        var initialRefreshToken = login!.Tokens.RefreshToken;

        // 2. Rotate Token
        var refreshResult = await _authService.RefreshTokenAsync(new RefreshTokenRequestDto
        {
            RefreshToken = initialRefreshToken
        });

        Assert.NotNull(refreshResult);
        Assert.False(string.IsNullOrWhiteSpace(refreshResult!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshResult.RefreshToken));
        Assert.NotEqual(initialRefreshToken, refreshResult.RefreshToken);

        // 3. Attempting to use the OLD (now revoked) refresh token must be rejected
        var reusedResult = await _authService.RefreshTokenAsync(new RefreshTokenRequestDto
        {
            RefreshToken = initialRefreshToken
        });
        Assert.Null(reusedResult);
    }

    [Fact]
    public async Task AuthService_RevokeToken_PreventsFurtherRotation()
    {
        var login = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = "user@isoflow.io",
            Password = "Test@123"
        });
        Assert.NotNull(login);

        var refreshToken = login!.Tokens.RefreshToken;

        // Revoke token (logout)
        var revoked = await _authService.RevokeTokenAsync(refreshToken);
        Assert.True(revoked);

        // Attempt to refresh with revoked token should fail
        var refresh = await _authService.RefreshTokenAsync(new RefreshTokenRequestDto
        {
            RefreshToken = refreshToken
        });
        Assert.Null(refresh);
    }

    [Fact]
    public async Task AuthService_GetUserProfile_ReturnsAccurateDetails()
    {
        var profile = await _authService.GetUserProfileByIdAsync("1");

        Assert.NotNull(profile);
        Assert.Equal("1", profile!.Id);
        Assert.Equal("admin@isoflow.io", profile.Email);
        Assert.Equal("System SuperAdmin", profile.Name);
    }

    #endregion

    #region Controller Endpoint Tests

    [Fact]
    public async Task AuthController_LoginEndpoint_Returns200WithPayload()
    {
        var controller = new AuthController(_authService, _jwtService);

        var actionResult = await controller.Login(new LoginRequestDto
        {
            Email = "admin@isoflow.io",
            Password = "Test@123"
        });

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var apiResponse = Assert.IsType<ApiResponse<LoginResponseDto>>(okResult.Value);

        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.False(string.IsNullOrWhiteSpace(apiResponse.Data!.Tokens.AccessToken));
    }

    [Fact]
    public async Task AuthController_LoginEndpoint_InvalidCredentials_Returns401()
    {
        var controller = new AuthController(_authService, _jwtService);

        var actionResult = await controller.Login(new LoginRequestDto
        {
            Email = "admin@isoflow.io",
            Password = "WrongPassword!"
        });

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
        var apiResponse = Assert.IsType<ApiResponse<LoginResponseDto>>(unauthorizedResult.Value);

        Assert.False(apiResponse.Success);
        Assert.Null(apiResponse.Data);
    }

    [Fact]
    public async Task AuthController_GetCurrentUser_WithClaimsPrincipal_ReturnsProfile()
    {
        var controller = new AuthController(_authService, _jwtService);

        // Simulate authenticated user context
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(JwtRegisteredClaimNames.Sub, "1"),
            new(ClaimTypes.Email, "admin@isoflow.io"),
            new(ClaimTypes.Role, "SuperAdmin")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var actionResult = await controller.GetCurrentUser();
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var apiResponse = Assert.IsType<ApiResponse<UserProfileDto>>(okResult.Value);

        Assert.True(apiResponse.Success);
        Assert.Equal("1", apiResponse.Data!.Id);
        Assert.Equal("admin@isoflow.io", apiResponse.Data.Email);
    }

    [Fact]
    public async Task UsersController_MeEndpoint_ReturnsUserProfile()
    {
        var controller = new UsersController(_userRepository, _authService, _jwtService);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "2"),
            new(JwtRegisteredClaimNames.Sub, "2"),
            new(ClaimTypes.Email, "user@isoflow.io"),
            new(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var actionResult = await controller.GetCurrentUser();
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var apiResponse = Assert.IsType<ApiResponse<UserProfileDto>>(okResult.Value);

        Assert.True(apiResponse.Success);
        Assert.Equal("2", apiResponse.Data!.Id);
        Assert.Equal("user@isoflow.io", apiResponse.Data.Email);
    }

    #endregion

    #region Mock Repositories Helper

    private class MockUserRepository : IUserRepository
    {
        private readonly List<User> _users = new()
        {
            new User
            {
                Id = "1",
                Name = "System SuperAdmin",
                Email = "admin@isoflow.io",
                Password = "Test@123",
                SystemRole = SystemRole.SuperAdmin,
                Role = "Super Administrator",
                Department = "Engineering",
                OrganizationId = "1"
            },
            new User
            {
                Id = "2",
                Name = "Compliance Officer",
                Email = "user@isoflow.io",
                Password = "Test@123",
                SystemRole = SystemRole.User,
                Role = "Compliance Manager",
                Department = "Compliance",
                OrganizationId = "1"
            }
        };

        public Task<List<User>> GetAllUsersAsync() => Task.FromResult(_users.ToList());
        public Task<PagedResponse<User>> GetPagedUsersAsync(PagedRequestDto request) => Task.FromResult(new PagedResponse<User>(_users, _users.Count, 1, 10));
        public Task<List<User>> GetUsersByOrganizationIdAsync(string organizationId) => Task.FromResult(_users.Where(u => u.OrganizationId == organizationId).ToList());
        public Task<PagedResponse<User>> GetPagedUsersByOrganizationIdAsync(string organizationId, PagedRequestDto request) => Task.FromResult(new PagedResponse<User>(_users.Where(u => u.OrganizationId == organizationId).ToList(), 1, 1, 10));
        public Task<User?> GetUserByIdAsync(string id) => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        public Task<User?> GetUserByEmailAsync(string email) => Task.FromResult(_users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        public Task<User> CreateUserAsync(User user) { user.Id = (_users.Count + 1).ToString(); _users.Add(user); return Task.FromResult(user); }
        public Task<User?> UpdateUserAsync(User user) { var idx = _users.FindIndex(u => u.Id == user.Id); if (idx >= 0) _users[idx] = user; return Task.FromResult<User?>(user); }
        public Task<bool> DeleteUserAsync(string id) { return Task.FromResult(_users.RemoveAll(u => u.Id == id) > 0); }
        public Task<User?> ValidateLoginAsync(string email, string password) { var u = _users.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase)); return Task.FromResult(u != null && PasswordSecurityHelper.VerifyPassword(password, u.Password) ? u : null); }
        public Task<string?> GenerateResetTokenAsync(string email) => Task.FromResult<string?>("RESET123");
        public Task<bool> ResetPasswordAsync(string token, string newPassword) => Task.FromResult(true);
        public Task<bool> UpdateProfileAsync(string id, string name, string phone, string department, string bio) => Task.FromResult(true);
        public Task<bool> ChangePasswordAsync(string id, string currentPassword, string newPassword) => Task.FromResult(true);
    }

    #endregion
}
