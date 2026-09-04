using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ISOFlow.Web.Filters;

/// <summary>
/// Session & Token Authorization Filter for MVC Controllers.
/// Ensures that requests possess an active JWT session token and satisfy role requirements.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class SessionAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    public string? Roles { get; set; }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Skip authorization if endpoint has [AllowAnonymous]
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous)
        {
            await next();
            return;
        }

        // 2. Verify active session and JWT token exist
        var session = context.HttpContext.Session;
        var token = session.GetString("ApiToken");
        var userId = session.GetString("ActiveUserId");

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userId))
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
            return;
        }

        // 3. Verify Role permissions if Roles are configured
        if (!string.IsNullOrWhiteSpace(Roles))
        {
            var userRole = session.GetString("ActiveUserSystemRole") ?? string.Empty;
            var allowedRoles = Roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (!allowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
            {
                if (context.Controller is Controller controller)
                {
                    controller.TempData["ErrorMessage"] = "Access Denied: You do not possess the required permissions for that area.";
                }
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }
        }

        await next();
    }
}
