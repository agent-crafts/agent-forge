using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Components.Delete;

public sealed class DeleteViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(DeleteViewModel model) =>
        View("~/Components/Delete/Default.cshtml", model);
}
