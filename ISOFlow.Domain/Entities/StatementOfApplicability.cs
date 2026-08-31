namespace ISOFlow.Domain.Entities;

public class StatementOfApplicability
{
    public string ControlId { get; set; } = string.Empty;
    public string ControlCode { get; set; } = string.Empty;
    public string ControlTitle { get; set; } = string.Empty;
    public bool Applicable { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string ImplementationStatus { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public int EvidenceCount { get; set; }
}
