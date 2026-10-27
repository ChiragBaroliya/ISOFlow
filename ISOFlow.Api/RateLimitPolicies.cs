namespace ISOFlow.Api;

/// <summary>Named rate-limiter policies registered in Program.cs.</summary>
public static class RateLimitPolicies
{
    /// <summary>
    /// Anonymous credential endpoints (login, refresh, forgot/reset password): fixed window per client IP,
    /// sized by RateLimiting:Auth:PermitLimit / WindowSeconds.
    /// </summary>
    public const string Auth = "auth";
}
