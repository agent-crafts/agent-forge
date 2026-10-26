using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("design-system")]
public sealed class DesignSystemController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Design System";
        ViewData["Section"] = "design-system";
        return View();
    }
}
