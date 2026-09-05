using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Entities;

public class TaskTemplate
{
    public string Id { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskFrequency Frequency { get; set; } = TaskFrequency.Monthly;
    public string DefaultOwner { get; set; } = string.Empty;
    public string RelatedControlId { get; set; } = string.Empty;
}
