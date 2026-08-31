namespace ISOFlow.Domain.Entities;

public class AuditProgram
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}
