namespace ISOFlow.Domain.Entities;

public class ManagementReview
{
    public string Id { get; set; } = string.Empty; // REV-2026-Q4
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Period { get; set; } = "Q4 2026";
    public DateTime ReviewDate { get; set; }
    public string ChairPerson { get; set; } = string.Empty;
    public List<string> Attendees { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed";
    public List<ManagementDecision> Decisions { get; set; } = new();
}
