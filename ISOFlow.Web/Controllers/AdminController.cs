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
        ViewData["ActiveTraceabilityId"] = "CTRL-001";
        var users = await _usersClient.GetAllUsersAsync();
        ViewBag.Organizations = await _orgsClient.GetAllOrganizationsAsync();
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(User user)
    {
        if (ModelState.IsValid)
        {
            if (user.SystemRole == SystemRole.SuperAdmin)
            {
                user.OrganizationId = "";
            }
            await _usersClient.CreateUserAsync(user);
            TempData["SuccessMessage"] = $"User '{user.Name}' created successfully!";
        }
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(User user)
    {
        if (ModelState.IsValid)
        {
            if (user.SystemRole == SystemRole.SuperAdmin)
            {
                user.OrganizationId = "";
            }
            var updated = await _usersClient.UpdateUserAsync(user);
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
