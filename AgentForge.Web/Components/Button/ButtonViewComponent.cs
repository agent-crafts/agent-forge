using Microsoft.AspNetCore.Mvc;
namespace AgentForge.Web.Components.Button;
public sealed class ButtonViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ButtonViewModel model) => View("~/Components/Button/Default.cshtml", model);
}
