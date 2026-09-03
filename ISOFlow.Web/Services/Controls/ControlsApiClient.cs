using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Controls;

public interface IControlsApiClient
{
    Task<List<Control>> GetAllControlsAsync();
    Task<Control?> GetControlByIdAsync(string id);
    Task<Control?> CreateControlAsync(Control control);
    Task<Control?> UpdateControlAsync(Control control);
    Task<bool> DeleteControlAsync(string id);
    Task<List<StatementOfApplicability>> GetStatementOfApplicabilityAsync();
    Task<RelatedItemsCountDto> GetRelatedItemsCountAsync(string controlId);
}

public class ControlsApiClient : IControlsApiClient
{
    private readonly IApiHttpClient _api;

    public ControlsApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<List<Control>> GetAllControlsAsync() =>
        await _api.GetAsync<List<Control>>("api/controls/all") ?? new();

    public async Task<Control?> GetControlByIdAsync(string id) =>
        await _api.GetAsync<Control>($"api/controls/{Uri.EscapeDataString(id)}");

    public async Task<Control?> CreateControlAsync(Control control) =>
        await _api.PostAsync<Control>("api/controls", new
        {
            control.Code, control.Title, control.RequirementId, control.StandardId,
            control.Category, control.Description, control.Status, control.Owner,
            control.CompliancePercentage, control.IsApplicable, control.Justification
        });

    public async Task<Control?> UpdateControlAsync(Control control) =>
        await _api.PutAsync<Control>($"api/controls/{Uri.EscapeDataString(control.Id)}", new
        {
            control.Code, control.Title, control.RequirementId, control.StandardId,
            control.Category, control.Description, control.Status, control.Owner,
            control.CompliancePercentage, control.IsApplicable, control.Justification
        });

    public async Task<bool> DeleteControlAsync(string id) =>
        await _api.DeleteAsync($"api/controls/{Uri.EscapeDataString(id)}");

    public async Task<List<StatementOfApplicability>> GetStatementOfApplicabilityAsync() =>
        await _api.GetAsync<List<StatementOfApplicability>>("api/controls/soa") ?? new();

    public async Task<RelatedItemsCountDto> GetRelatedItemsCountAsync(string controlId) =>
        await _api.GetAsync<RelatedItemsCountDto>($"api/controls/{Uri.EscapeDataString(controlId)}/related-items") ?? new();
}
