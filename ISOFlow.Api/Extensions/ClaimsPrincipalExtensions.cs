using System.Security.Claims;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Api.Extensions;

/// <summary>
/// Derives the acting user's identity/role/tenant strictly from validated JWT claims.
/// Controllers must use these instead of trusting a client-supplied id in the route,
/// query string, or request body.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");

    public static string? GetOrganizationId(this ClaimsPrincipal user) =>
        user.FindFirstValue("org_id");

    /// <summary>
    /// Resolves the caller's organization id as an <see cref="int"/> for tenant-scoped filtering.
    /// Returns <c>null</c> when the caller is SuperAdmin (meaning "no org filter — see everything")
    /// or when the "org_id" claim is missing/empty/unparseable. This is the single centralized place
    /// the "SuperAdmin sees everything" rule lives — callers should use this instead of re-deriving it.
    /// </summary>
    public static int? GetOrganizationIdOrNull(this ClaimsPrincipal user)
    {
        if (user.IsSuperAdmin()) return null;

        var raw = user.GetOrganizationId();
        return !string.IsNullOrWhiteSpace(raw) && int.TryParse(raw, out var organizationId) ? organizationId : null;
    }

    public static SystemRole? GetSystemRole(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.Role) ?? user.FindFirstValue("role");
        return Enum.TryParse<SystemRole>(raw, out var role) ? role : null;
    }

    public static bool IsSuperAdmin(this ClaimsPrincipal user) => user.GetSystemRole() == SystemRole.SuperAdmin;

    public static bool IsAdmin(this ClaimsPrincipal user) => user.GetSystemRole() == SystemRole.Admin;

    /// <summary>True when <paramref name="targetUserId"/> is the caller's own id.</summary>
    public static bool IsSelf(this ClaimsPrincipal user, string? targetUserId) =>
        !string.IsNullOrWhiteSpace(targetUserId) &&
        string.Equals(user.GetUserId(), targetUserId, StringComparison.Ordinal);

    /// <summary>True when <paramref name="targetOrganizationId"/> matches the caller's own org.</summary>
    public static bool IsSameOrganization(this ClaimsPrincipal user, string? targetOrganizationId) =>
        !string.IsNullOrWhiteSpace(targetOrganizationId) &&
        string.Equals(user.GetOrganizationId(), targetOrganizationId, StringComparison.OrdinalIgnoreCase);
}
