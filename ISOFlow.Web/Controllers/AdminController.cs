using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Web.Filters;
using ISOFlow.Web.Services.Organizations;
using ISOFlow.Web.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

[SessionAuthorize(Roles = "SuperAdmin,Admin")]
public class AdminController : Controller
{
    private readonly IUsersApiClient _usersClient;
    private readonly IOrganizationsApiClient _orgsClient;

    public AdminController(IUsersApiClient usersClient, IOrganizationsApiClient orgsClient)
    {
        _usersClient = usersClient;
        _orgsClient = orgsClient;
    }

    public async Task<IActionResult> Users()
    {
        ViewData["ActiveMenu"] = "Users";
        var users = await _usersClient.GetAllUsersAsync();
        ViewBag.Organizations = await _orgsClient.GetAllOrganizationsAsync();
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(User user, string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            TempData["ErrorMessage"] = "An initial password of at least 6 characters is required.";
            return RedirectToAction(nameof(Users));
        }

        if (ModelState.IsValid)
        {
            if (user.SystemRole == SystemRole.SuperAdmin)
            {
                user.OrganizationId = null;
            }
            var created = await _usersClient.CreateUserAsync(user, password);
            if (created != null)
            {
                TempData["SuccessMessage"] = $"User '{user.Name}' created successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Unable to create user '{user.Name}'.";
            }
        }
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(User user, string? newPassword)
    {
        if (!string.IsNullOrWhiteSpace(newPassword) && newPassword.Length < 6)
        {
            TempData["ErrorMessage"] = "A new password must be at least 6 characters.";
            return RedirectToAction(nameof(Users));
        }

        if (ModelState.IsValid)
        {
            if (user.SystemRole == SystemRole.SuperAdmin)
            {
                user.OrganizationId = null;
            }
            var updated = await _usersClient.UpdateUserAsync(user, newPassword);
            if (updated != null)
            {
                TempData["SuccessMessage"] = $"User account '{updated.Name}' updated successfully.";
            }
        }
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var currentUserId = HttpContext.Session.GetString("ActiveUserId");
        if (id.Equals(currentUserId, StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You cannot delete your own active account.";
            return RedirectToAction(nameof(Users));
        }

        var result = await _usersClient.DeleteUserAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "User account removed.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete user.";
        }
        return RedirectToAction(nameof(Users));
    }
}
