namespace ISOFlow.Domain.Entities;

public class CapaActionItem
{
    public string Id { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public string CapaId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
}
