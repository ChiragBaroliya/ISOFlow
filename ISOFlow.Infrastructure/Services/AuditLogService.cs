using ISOFlow.Application.Helpers;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;

namespace ISOFlow.Infrastructure.Services;

/// <inheritdoc cref="IAuditLogService"/>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> LogAsync(AuditEntryContext context)
    {
        string? oldValuesJson = null;
        string? newValuesJson = null;
        string? changedFieldsJson = null;

        switch (context.Action)
        {
            case AuditActionType.Create:
                newValuesJson = EntityDiffHelper.ToRedactedJson(context.NewEntity);
                break;

            case AuditActionType.Delete:
                oldValuesJson = EntityDiffHelper.ToRedactedJson(context.OldEntity);
                break;

            case AuditActionType.Update:
            {
                var changes = EntityDiffHelper.Diff(context.OldEntity, context.NewEntity);
                if (changes.Count == 0)
                {
                    // A save that produced no actual field difference is not a meaningful change.
                    return false;
                }

                oldValuesJson = EntityDiffHelper.ToRedactedJson(context.OldEntity);
                newValuesJson = EntityDiffHelper.ToRedactedJson(context.NewEntity);
                changedFieldsJson = EntityDiffHelper.ToJson(changes);
                break;
            }

            default:
                // Named business actions (Approve/Reject/Submit/Assign/Unassign/Activate/Deactivate/
                // StatusChange/BulkUpdate) are meaningful by virtue of having happened, regardless of
                // whether the snapshot diff is empty — always record them when invoked.
                oldValuesJson = EntityDiffHelper.ToRedactedJson(context.OldEntity);
                newValuesJson = EntityDiffHelper.ToRedactedJson(context.NewEntity);
                if (context.OldEntity != null && context.NewEntity != null)
                {
                    var changes = EntityDiffHelper.Diff(context.OldEntity, context.NewEntity);
                    if (changes.Count > 0) changedFieldsJson = EntityDiffHelper.ToJson(changes);
                }
                break;
        }

        await _repository.InsertAsync(new AuditLog
        {
            OrganizationId = context.OrganizationId,
            ModuleName = context.ModuleName,
            EntityName = context.EntityName,
            EntityId = context.EntityId,
            Action = context.Action,
            PerformedBy = context.PerformedBy,
            PerformedAt = DateTime.UtcNow,
            IpAddress = context.IpAddress,
            UserAgent = context.UserAgent,
            CorrelationId = context.CorrelationId,
            Description = context.Description,
            OldValues = oldValuesJson,
            NewValues = newValuesJson,
            ChangedFields = changedFieldsJson
        });

        return true;
    }
}
