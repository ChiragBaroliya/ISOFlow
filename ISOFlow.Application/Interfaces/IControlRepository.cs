using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IControlRepository
{
    Task<List<Control>> GetAllControlsAsync();
    Task<Control?> GetControlByIdAsync(string id);
    Task<List<StatementOfApplicability>> GetStatementOfApplicabilityAsync();
    Task<RelatedItemsCountDto> GetRelatedItemsCountAsync(string controlId);
    Task<Control> CreateControlAsync(Control control);
    Task<Control?> UpdateControlAsync(Control control);
    Task<bool> DeleteControlAsync(string id);
}
