using ISOFlow.Domain.Enums;

namespace ISOFlow.Domain.Entities;

public class CAPA
{
    public string Id { get; set; } = string.Empty; // CAPA-001
    public string Code { get; set; } = string.Empty;
    public string FindingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public string CorrectiveAction { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public CapaStatus Status { get; set; }
    public string EffectivenessVerification { get; set; } = string.Empty;
    public List<CapaActionItem> ActionItems { get; set; } = new();
}
