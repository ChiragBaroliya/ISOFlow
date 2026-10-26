using ISOFlow.Web.Services.Traceability;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Components;

/// <summary>
/// Renders the compliance traceability chain for the entity the current page is about
/// (ViewData["ActiveTraceabilityId"]). Renders nothing when the page sets no entity or the
/// entity has no linked chain.
/// </summary>
public class TraceabilityBarViewComponent : ViewComponent
{
    private readonly ITraceabilityApiClient _traceabilityClient;

    public TraceabilityBarViewComponent(ITraceabilityApiClient traceabilityClient)
    {
        _traceabilityClient = traceabilityClient;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var entityCode = ViewData["ActiveTraceabilityId"]?.ToString();
        if (string.IsNullOrWhiteSpace(entityCode))
            return Content(string.Empty);

        var chain = await _traceabilityClient.GetChainAsync(entityCode);
        if (chain.Steps.Count == 0)
            return Content(string.Empty);

        return View(chain);
    }
}
