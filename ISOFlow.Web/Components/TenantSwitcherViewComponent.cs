using ISOFlow.Web.Services.Organizations;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Web.Components;

/// <summary>SuperAdmin tenant switcher dropdown listing the organizations returned by the API.</summary>
public class TenantSwitcherViewComponent : ViewComponent
{
    private readonly IOrganizationsApiClient _orgsClient;

    public TenantSwitcherViewComponent(IOrganizationsApiClient orgsClient)
    {
        _orgsClient = orgsClient;
    }

    public async Task<IViewComponentResult> InvokeAsync(string activeOrgCode)
    {
        ViewData["ActiveOrgCode"] = activeOrgCode;
        var organizations = await _orgsClient.GetAllOrganizationsAsync();
        return View(organizations.OrderBy(o => o.Name).ToList());
    }
}
