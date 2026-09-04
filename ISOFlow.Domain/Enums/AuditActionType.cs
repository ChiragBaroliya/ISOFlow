namespace ISOFlow.Domain.Enums;

/// <summary>Kinds of meaningful business/data changes captured by the centralized audit log.</summary>
public enum AuditActionType
{
    Create,
    Update,
    Delete,
    Approve,
    Reject,
    Submit,
    Assign,
    Unassign,
    Activate,
    Deactivate,
    StatusChange,
    BulkUpdate
}
