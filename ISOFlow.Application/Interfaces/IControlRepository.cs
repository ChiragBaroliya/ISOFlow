using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IControlRepository
{
    Task<List<Control>> GetAllControlsAsync(int? organizationId);
    Task<PagedResponse<Control>> GetPagedControlsAsync(PagedRequestDto request, int? organizationId);
    Task<Control?> GetControlByIdAsync(string id, int? organizationId);
    Task<List<StatementOfApplicability>> GetStatementOfApplicabilityAsync(int? organizationId);
    Task<RelatedItemsCountDto> GetRelatedItemsCountAsync(string controlId, int? organizationId);
    Task<Control> CreateControlAsync(Control control);
    Task<Control?> UpdateControlAsync(Control control, int organizationId);
    Task<bool> DeleteControlAsync(string id, int organizationId);
}
