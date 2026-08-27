using ISOFlow.Domain.Entities;

namespace ISOFlow.Infrastructure.MockData;

public static partial class MockStore
{
    public static List<Notification> Notifications { get; } = new()
    {
        new Notification { Id = "NOTIF-001", Title = "Task Overdue",       Message = "5 compliance tasks require your immediate action.", Category = "Warning", CreatedAt = DateTime.UtcNow.AddHours(-2), IsRead = false, LinkUrl = "/Tasks" },
        new Notification { Id = "NOTIF-002", Title = "Policy Review Due",  Message = "Access Control Policy requires annual review.",       Category = "Policy",  CreatedAt = DateTime.UtcNow.AddHours(-5), IsRead = false, LinkUrl = "/Documents/Policies" },
        new Notification { Id = "NOTIF-003", Title = "New Audit Finding",  Message = "FIND-001 Access Not Removed logged for AUD-2026-001.", Category = "Audit",  CreatedAt = DateTime.UtcNow.AddDays(-1),  IsRead = true,  LinkUrl = "/Findings/Detail?id=FIND-001" }
    };

    public static List<AuditLog> AuditLogs { get; } = new()
    {
        new AuditLog { Id = "LOG-001", Timestamp = DateTime.UtcNow.AddHours(-1), User = "Chirag Baroliya", Action = "Updated Control", Module = "Controls", EntityId = "CTRL-001", Details = "Updated compliance score to 82% after Q3 Access Review." },
        new AuditLog { Id = "LOG-002", Timestamp = DateTime.UtcNow.AddHours(-3), User = "Sarah Wilson",    Action = "Logged Finding",  Module = "Audit",    EntityId = "FIND-001", Details = "Created Major Non-Conformity FIND-001 for AUD-2026-001." }
    };
}
