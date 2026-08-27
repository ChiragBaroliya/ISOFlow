using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Components;

public class RelatedComplianceItemsViewComponent : ViewComponent
{
    private readonly IControlRepository _controlRepository;

    public RelatedComplianceItemsViewComponent(IControlRepository controlRepository)
    {
        _controlRepository = controlRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(string controlId = "CTRL-001")
    {
        var counts = await _controlRepository.GetRelatedItemsCountAsync(controlId);
        return View(counts);
    }
}
