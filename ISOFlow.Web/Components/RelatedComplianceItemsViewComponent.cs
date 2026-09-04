using ISOFlow.Application.DTOs;
using ISOFlow.Web.Services.Controls;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Components;

public class RelatedComplianceItemsViewComponent : ViewComponent
{
    private readonly IControlsApiClient _controlsClient;

    public RelatedComplianceItemsViewComponent(IControlsApiClient controlsClient)
    {
        _controlsClient = controlsClient;
    }

    public async Task<IViewComponentResult> InvokeAsync(string controlId = "CTRL-001")
    {
        var counts = await _controlsClient.GetRelatedItemsCountAsync(controlId);
        return View(counts);
    }
}

