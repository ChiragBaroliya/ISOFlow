using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Controllers;

public class AdminController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _orgRepository;

    public AdminController(IUserRepository userRepository, IOrganizationRepository orgRepository)
    {
        _userRepository = userRepository;
        _orgRepository = orgRepository;
    }

    public async Task<IActionResult> Users()
    {
        ViewData["ActiveMenu"] = "Users";
        ViewData["ActiveTraceabilityId"] = "CTRL-001";
        var users = await _userRepository.GetAllUsersAsync();
        ViewBag.Organizations = await _orgRepository.GetAllOrganizationsAsync();
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
            await _userRepository.CreateUserAsync(user);
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
            var updated = await _userRepository.UpdateUserAsync(user);
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

        var result = await _userRepository.DeleteUserAsync(id);
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
