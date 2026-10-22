using AgentForge.Application.Tools;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

/// <summary>Read-only catalog. Tool execution is deliberately not exposed by this controller.</summary>
[Route("tools")]
public sealed class ToolsController(IToolRegistry registry) : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Tools";
        ViewData["Section"] = "tools";
        ViewData["Description"] = "Discover registered agent capabilities.";
        var tools = registry.GetTools()
            .Select(tool => new ToolCatalogItem(tool.Name, tool.Description ?? "",
                tool.JsonSchema.ToString()))
            .ToArray();
        return View(tools);
    }

    public sealed record ToolCatalogItem(string Name, string Description, string JsonSchema);
}
