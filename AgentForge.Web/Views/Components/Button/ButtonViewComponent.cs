using Microsoft.AspNetCore.Mvc;
namespace AgentForge.Web.Views.Components.Button;
public sealed class ButtonViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ButtonViewModel model) => View("~/Views/Components/Button/Default.cshtml", model);
}
