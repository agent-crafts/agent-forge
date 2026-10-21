using System.Diagnostics;
using AgentForge.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

public sealed class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => RedirectToAction("Index", "Chat");

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
