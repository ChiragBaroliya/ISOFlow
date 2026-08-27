using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.MockData;

namespace ISOFlow.Infrastructure.Repositories;

public class ControlRepository : IControlRepository
{
    public Task<List<Control>> GetAllControlsAsync() => Task.FromResult(MockStore.Controls);

    public Task<Control?> GetControlByIdAsync(string id) =>
        Task.FromResult(MockStore.Controls.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || c.Code.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<List<StatementOfApplicability>> GetStatementOfApplicabilityAsync()
    {
        var soa = MockStore.Controls.Select(c => new StatementOfApplicability
        {
            ControlId = c.Id,
            ControlCode = c.Code,
            ControlTitle = c.Title,
            Applicable = c.IsApplicable,
            Justification = c.Justification,
            ImplementationStatus = c.Status.ToString(),
            Owner = c.Owner,
            EvidenceCount = c.RelatedEvidenceIds.Count
        }).ToList();

        return Task.FromResult(soa);
    }

    public Task<RelatedItemsCountDto> GetRelatedItemsCountAsync(string controlId)
    {
        var control = MockStore.Controls.FirstOrDefault(c => c.Id.Equals(controlId, StringComparison.OrdinalIgnoreCase));
        if (control == null) return Task.FromResult(new RelatedItemsCountDto());

        var dto = new RelatedItemsCountDto
        {
            Requirements = control.RelatedRequirementIds.Count,
            Controls = 1,
            Risks = control.RelatedRiskIds.Count,
            Treatments = control.RelatedTreatmentIds.Count,
            Policies = control.RelatedPolicyIds.Count,
            Processes = control.RelatedProcessIds.Count,
            Tasks = control.RelatedTaskIds.Count,
            Evidence = control.RelatedEvidenceIds.Count,
            Audits = control.RelatedAuditIds.Count,
            Findings = control.RelatedFindingIds.Count,
            Capa = control.RelatedCapaIds.Count,
            Improvements = control.RelatedImprovementIds.Count
        };

        return Task.FromResult(dto);
    }

    public Task<Control> CreateControlAsync(Control control)
    {
        if (string.IsNullOrWhiteSpace(control.Id))
        {
            control.Id = MockStore.NextId("CTRL", MockStore.Controls.Count);
        }
        if (string.IsNullOrWhiteSpace(control.Code))
        {
            control.Code = control.Id;
        }
        MockStore.Controls.Add(control);
        return Task.FromResult(control);
    }

    public Task<Control?> UpdateControlAsync(Control control)
    {
        var existing = MockStore.Controls.FirstOrDefault(c => c.Id.Equals(control.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Title = control.Title;
            existing.StandardId = control.StandardId;
            existing.RequirementId = control.RequirementId;
            existing.Category = control.Category;
            existing.Description = control.Description;
            existing.Status = control.Status;
            existing.Owner = control.Owner;
            existing.CompliancePercentage = control.CompliancePercentage;
            existing.IsApplicable = control.IsApplicable;
            existing.Justification = control.Justification;
        }
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteControlAsync(string id)
    {
        var existing = MockStore.Controls.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            MockStore.Controls.Remove(existing);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
