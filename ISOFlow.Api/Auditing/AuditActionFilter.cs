using System.Data.Common;
using System.Security.Claims;
using ISOFlow.Api.Extensions;
using ISOFlow.Application.Interfaces;
using ISOFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ISOFlow.Api.Auditing;

/// <summary>
/// Global action filter that makes audit logging automatic. It runs around EVERY controller
/// action; for the ones decorated with <see cref="AuditAttribute"/> it:
///  1. Snapshots the entity's "before" state (via <see cref="IAuditSnapshotRegistry"/>), if applicable.
///  2. Opens one database transaction and marks it as the ambient transaction (<see cref="AmbientDbContext"/>)
///     for the rest of this request — every repository call the action makes (via BaseRepository)
///     transparently joins this same transaction, with no repository code changes required.
///  3. Runs the action.
///  4. On success, snapshots the "after" state, diffs it against the "before" state, and writes the
///     audit entry through <see cref="IAuditLogService"/> — on the SAME transaction.
///  5. Commits only if both the business write and the audit write succeeded; otherwise the
///     transaction is rolled back (or simply never committed, which disposal rolls back for us),
///     so a failed audit write undoes the business change and vice versa.
/// Actions without <see cref="AuditAttribute"/> are completely unaffected — they run exactly as
/// before, opening their own per-call connections outside any ambient transaction.
/// </summary>
public class AuditActionFilter : IAsyncActionFilter
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IAuditLogService _auditLogService;
    private readonly IAuditSnapshotRegistry _snapshotRegistry;
    private readonly ILogger<AuditActionFilter> _logger;

    public AuditActionFilter(
        IDbConnectionFactory dbConnectionFactory,
        IAuditLogService auditLogService,
        IAuditSnapshotRegistry snapshotRegistry,
        ILogger<AuditActionFilter> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _auditLogService = auditLogService;
        _snapshotRegistry = snapshotRegistry;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var auditAttribute = ResolveAttribute(context);
        if (auditAttribute == null)
        {
            await next();
            return;
        }

        var httpContext = context.HttpContext;
        var serviceProvider = httpContext.RequestServices;
        var routeEntityId = ResolveRouteId(context, auditAttribute.IdParameter);

        var oldEntity = auditAttribute.CaptureOldValue && routeEntityId != null
            ? await _snapshotRegistry.ResolveAsync(serviceProvider, auditAttribute.Entity, routeEntityId)
            : null;

        using var connection = (DbConnection)_dbConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();
        using var ambientScope = AmbientDbContext.Begin(connection, transaction);

        var executedContext = await next();

        var businessSucceeded = executedContext.Exception == null || executedContext.ExceptionHandled;
        if (!businessSucceeded || !TryGetSuccessBody(executedContext.Result, out var responseBody))
        {
            // Business operation failed, was rejected, or returned no content to audit — nothing was
            // meaningfully persisted, so roll back (a no-op for actions that didn't write anything)
            // and skip writing an audit entry for it.
            await transaction.RollbackAsync();
            return;
        }

        var entityId = routeEntityId ?? ResolveIdFromResponse(responseBody) ?? string.Empty;

        object? newEntity = null;
        if (auditAttribute.CaptureNewValue)
        {
            newEntity = !string.IsNullOrEmpty(entityId)
                ? await _snapshotRegistry.ResolveAsync(serviceProvider, auditAttribute.Entity, entityId)
                : null;
            newEntity ??= ExtractApiResponseData(responseBody);
        }

        try
        {
            await _auditLogService.LogAsync(new AuditEntryContext
            {
                ModuleName = auditAttribute.Module,
                EntityName = auditAttribute.Entity,
                EntityId = entityId,
                Action = auditAttribute.Action,
                OldEntity = oldEntity,
                NewEntity = newEntity,
                TenantId = httpContext.User.GetOrganizationId() ?? string.Empty,
                PerformedBy = ResolvePerformedBy(httpContext.User),
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
                CorrelationId = httpContext.TraceIdentifier
            });

            // Reaching here means both the business write and the audit write succeeded — commit both
            // together.
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            // The `using` declarations below still dispose the (uncommitted) transaction, rolling
            // back the business change too — audit failure must not leave a half-recorded change.
            _logger.LogError(ex, "Audit write failed for {Module}/{Entity} {Action} (id={EntityId}) — rolling back the business change with it.",
                auditAttribute.Module, auditAttribute.Entity, auditAttribute.Action, entityId);
            throw;
        }
    }

    private static AuditAttribute? ResolveAttribute(ActionExecutingContext context) =>
        (context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo
            .GetCustomAttributes(typeof(AuditAttribute), inherit: false)
            .Cast<AuditAttribute>()
            .FirstOrDefault();

    private static string? ResolveRouteId(ActionExecutingContext context, string idParameter) =>
        context.ActionArguments.TryGetValue(idParameter, out var value) && value != null
            ? value.ToString()
            : null;

    private static bool TryGetSuccessBody(IActionResult? result, out object? body)
    {
        switch (result)
        {
            case ObjectResult objectResult when (objectResult.StatusCode ?? 200) is >= 200 and < 300:
                body = objectResult.Value;
                return true;
            case OkResult:
            case StatusCodeResult { StatusCode: >= 200 and < 300 }:
                body = null;
                return true;
            default:
                body = null;
                return false;
        }
    }

    private static object? ExtractApiResponseData(object? body) =>
        body?.GetType().GetProperty("Data")?.GetValue(body);

    private static string? ResolveIdFromResponse(object? body)
    {
        var data = ExtractApiResponseData(body);
        var idValue = data?.GetType().GetProperty("Id")?.GetValue(data);
        return idValue?.ToString();
    }

    private static string ResolvePerformedBy(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Email)
        ?? user.GetUserId()
        ?? "unknown";
}
