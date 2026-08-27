using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class AccountController : Controller
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;

    public AccountController(IOrganizationRepository organizationRepository, IUserRepository userRepository)
    {
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  LOGIN
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("ActiveUserId")))
            return RedirectToAction("Index", "Home");

        ViewData["Title"] = "Sign In — ISOFlow";
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
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

        var user = await _userRepository.ValidateLoginAsync(email, password);

        if (user == null)
        {
            RenderLoginError("Invalid email address or password. Please try again.");
            return View();
        }

        // ── Set session ───────────────────────────────────────────────────────
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
            var org = await _organizationRepository.GetOrganizationByIdAsync(user.OrganizationId);
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
    public IActionResult Logout()
    {
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

        var org = await _organizationRepository.GetOrganizationByIdAsync(orgId);
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
    public IActionResult ForgotPassword()
    {
        ViewData["Title"] = "Forgot Password — ISOFlow";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        ViewData["Title"] = "Forgot Password — ISOFlow";

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("", "Please enter your registered email address.");
            return View();
        }

        var token = await _userRepository.GenerateResetTokenAsync(email.Trim());

        // We show success regardless of whether the email exists (security best practice).
        // In mock mode we additionally surface the token so the user can proceed immediately.
        ViewBag.EmailSent = true;
        ViewBag.MockToken = token;    // null if email not found
        ViewBag.Email     = email.Trim();
        return View();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  RESET PASSWORD
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult ResetPassword(string? token = null)
    {
        ViewData["Title"] = "Reset Password — ISOFlow";
        ViewBag.Token = token ?? "";
        return View();
    }

    [HttpPost]
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

        var success = await _userRepository.ResetPasswordAsync(token.Trim(), newPassword);

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

        var user = await _userRepository.GetUserByIdAsync(userId);
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

        var success = await _userRepository.UpdateProfileAsync(userId, name.Trim(), phone?.Trim() ?? "", department?.Trim() ?? "", bio?.Trim() ?? "");

        if (success)
        {
            // Update session name so the topbar reflects the change immediately
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

        var success = await _userRepository.ChangePasswordAsync(userId, currentPassword, newPassword);

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
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied — ISOFlow";
        return View();
    }
}
