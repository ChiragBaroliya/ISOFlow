using ISOFlow.Domain.Enums;
using ISOFlow.Web.Services.Auth;
using ISOFlow.Web.Services.Organizations;
using ISOFlow.Web.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthApiClient _authClient;
    private readonly IUsersApiClient _usersClient;
    private readonly IOrganizationsApiClient _orgsClient;

    public AccountController(
        IAuthApiClient authClient,
        IUsersApiClient usersClient,
        IOrganizationsApiClient orgsClient)
    {
        _authClient = authClient;
        _usersClient = usersClient;
        _orgsClient = orgsClient;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  LOGIN
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null, bool sessionExpired = false)
    {
        if (sessionExpired)
        {
            ViewBag.SessionExpired = true;
        }

        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("ActiveUserId")) && !sessionExpired)
            return RedirectToAction("Index", "Home");

        ViewData["Title"] = "Sign In — ISOFlow";
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        void RenderLoginError(string msg)
        {
            ViewData["Title"] = "Sign In — ISOFlow";
            ViewBag.ReturnUrl = returnUrl;
            ModelState.AddModelError("", msg);
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            RenderLoginError("Email and password are required.");
            return View();
        }

        // Authenticate via ISOFlow.Api
        var loginResult = await _authClient.LoginAsync(email.Trim(), password);

        if (loginResult == null || loginResult.User == null)
        {
            RenderLoginError("Invalid email address or password. Please try again.");
            return View();
        }

        var user = loginResult.User;
        var token = loginResult.Tokens?.AccessToken ?? "";
        var refreshToken = loginResult.Tokens?.RefreshToken ?? "";

        // ── Set session ───────────────────────────────────────────────────────
        HttpContext.Session.SetString("ApiToken",             token);
        HttpContext.Session.SetString("RefreshToken",         refreshToken);
        HttpContext.Session.SetString("ActiveUserId",         user.Id);
        HttpContext.Session.SetString("ActiveUserName",       user.Name);
        HttpContext.Session.SetString("ActiveUserEmail",      user.Email);
        HttpContext.Session.SetString("ActiveUserRole",       user.Role);
        HttpContext.Session.SetString("ActiveUserSystemRole", user.SystemRole.ToString());
        HttpContext.Session.SetString("ActiveUserAvatar",     user.AvatarUrl);
        HttpContext.Session.SetString("ActiveUserDept",       user.Department);
        HttpContext.Session.SetString("ActiveUserPhone",      user.Phone);

        if (user.SystemRole == SystemRole.SuperAdmin)
        {
            HttpContext.Session.SetString("ActiveOrgId",   "");
            HttpContext.Session.SetString("ActiveOrgName", "ISOFlow Platform");
            HttpContext.Session.SetString("ActiveOrgCode", "GLOBAL");
        }
        else
        {
            var org = await _orgsClient.GetOrganizationByIdAsync(user.OrganizationId);
            HttpContext.Session.SetString("ActiveOrgId",   org?.Id   ?? user.OrganizationId);
            HttpContext.Session.SetString("ActiveOrgName", org?.Name ?? "Unknown Organization");
            HttpContext.Session.SetString("ActiveOrgCode", org?.Code ?? "N/A");
        }

        TempData["SuccessMessage"] = $"Welcome back, {user.Name}!";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  LOGOUT
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = HttpContext.Session.GetString("RefreshToken");
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _authClient.LogoutAsync(refreshToken);
        }

        HttpContext.Session.Clear();
        TempData["SuccessMessage"] = "You have been signed out successfully.";
        return RedirectToAction(nameof(Login));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  SWITCH TENANT  (SuperAdmin only)
    // ─────────────────────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> SwitchTenant(string orgId, string? returnUrl = null)
    {
        var systemRole = HttpContext.Session.GetString("ActiveUserSystemRole");
        if (systemRole != nameof(SystemRole.SuperAdmin))
        {
            TempData["ErrorMessage"] = "Only Super Admins can switch organization tenants.";
            return RedirectToAction("Index", "Home");
        }

        var org = await _orgsClient.GetOrganizationByIdAsync(orgId);
        if (org != null)
        {
            HttpContext.Session.SetString("ActiveOrgId",   org.Id);
            HttpContext.Session.SetString("ActiveOrgName", org.Name);
            HttpContext.Session.SetString("ActiveOrgCode", org.Code);
            TempData["SuccessMessage"] = $"Switched active tenant to: {org.Name} ({org.Code})";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  FORGOT PASSWORD
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        ViewData["Title"] = "Forgot Password — ISOFlow";
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        ViewData["Title"] = "Forgot Password — ISOFlow";

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("", "Please enter your registered email address.");
            return View();
        }

        var token = await _authClient.ForgotPasswordAsync(email.Trim());

        ViewBag.EmailSent = true;
        ViewBag.MockToken = token;
        ViewBag.Email     = email.Trim();
        return View();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  RESET PASSWORD
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? token = null)
    {
        ViewData["Title"] = "Reset Password — ISOFlow";
        ViewBag.Token = token ?? "";
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
    {
        ViewData["Title"] = "Reset Password — ISOFlow";
        ViewBag.Token = token;

        if (string.IsNullOrWhiteSpace(token))
        {
            ModelState.AddModelError("", "Reset token is required.");
            return View();
        }
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            ModelState.AddModelError("", "New password must be at least 6 characters.");
            return View();
        }
        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError("", "Passwords do not match.");
            return View();
        }

        var success = await _authClient.ResetPasswordAsync(token.Trim(), newPassword);

        if (!success)
        {
            ModelState.AddModelError("", "Invalid or expired reset token. Please request a new one.");
            return View();
        }

        TempData["SuccessMessage"] = "Your password has been reset. Please sign in with your new password.";
        return RedirectToAction(nameof(Login));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PROFILE
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = HttpContext.Session.GetString("ActiveUserId");
        if (string.IsNullOrEmpty(userId)) return RedirectToAction(nameof(Login));

        var user = await _usersClient.GetUserByIdAsync(userId);
        if (user == null) return RedirectToAction(nameof(Login));

        ViewData["Title"]       = "My Profile — ISOFlow";
        ViewData["ActiveMenu"]  = "Profile";
        return View(user);
    }

    [HttpPost]
    [ActionName("Profile")]
    public async Task<IActionResult> ProfileSave(string name, string phone, string department, string bio)
    {
        var userId = HttpContext.Session.GetString("ActiveUserId");
        if (string.IsNullOrEmpty(userId)) return RedirectToAction(nameof(Login));

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "Name cannot be empty.";
            return RedirectToAction(nameof(Profile));
        }

        var success = await _usersClient.UpdateProfileAsync(userId, name.Trim(), phone?.Trim() ?? "", department?.Trim() ?? "", bio?.Trim() ?? "");

        if (success)
        {
            HttpContext.Session.SetString("ActiveUserName", name.Trim());
            HttpContext.Session.SetString("ActiveUserDept", department?.Trim() ?? "");
            TempData["SuccessMessage"] = "Profile updated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update profile. Please try again.";
        }

        return RedirectToAction(nameof(Profile));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CHANGE PASSWORD  (from Profile page)
    // ─────────────────────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var userId = HttpContext.Session.GetString("ActiveUserId");
        if (string.IsNullOrEmpty(userId)) return RedirectToAction(nameof(Login));

        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            TempData["ErrorMessage"] = "All password fields are required.";
            return RedirectToAction(nameof(Profile));
        }
        if (newPassword.Length < 6)
        {
            TempData["ErrorMessage"] = "New password must be at least 6 characters.";
            return RedirectToAction(nameof(Profile));
        }
        if (newPassword != confirmPassword)
        {
            TempData["ErrorMessage"] = "New passwords do not match.";
            return RedirectToAction(nameof(Profile));
        }

        var success = await _usersClient.ChangePasswordAsync(userId, currentPassword, newPassword);

        if (success)
            TempData["SuccessMessage"] = "Password changed successfully. Please use your new password next time you sign in.";
        else
            TempData["ErrorMessage"] = "Current password is incorrect.";

        return RedirectToAction(nameof(Profile));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ACCESS DENIED
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied — ISOFlow";
        return View();
    }
}
