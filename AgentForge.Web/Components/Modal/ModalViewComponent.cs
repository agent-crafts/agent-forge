using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Components.Modal;

public sealed class ModalViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ModalViewModel model) =>
        View("~/Components/Modal/Default.cshtml", model);
}
