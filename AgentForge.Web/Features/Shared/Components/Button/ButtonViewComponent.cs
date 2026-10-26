using Microsoft.AspNetCore.Mvc;
namespace AgentForge.Web.Features.Shared.Components.Button;
public sealed class ButtonViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ButtonViewModel model) => View("Default", model);
}
