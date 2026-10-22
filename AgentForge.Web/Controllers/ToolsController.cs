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
        SetPage("Tools", "Explore and manage tools available for your projects.");
        var tools = registry.GetTools()
            .Select(tool => new ToolCatalogItem(tool.Name, tool.Description ?? "", tool.JsonSchema.ToString()))
            .ToArray();
        return View(tools);
    }

    [HttpGet("{name}")]
    public IActionResult Details(string name)
    {
        var tool = registry.GetTools().FirstOrDefault(
            candidate => string.Equals(candidate.Name, name, StringComparison.Ordinal));
        if (tool is null) return NotFound();

        SetPage("Tool Details", "Review the tool's description and input contract.");
        return View(new ToolCatalogItem(tool.Name, tool.Description ?? "", tool.JsonSchema.ToString()));
    }

    private void SetPage(string title, string description)
    {
        ViewData["Title"] = title;
        ViewData["Section"] = "tools";
        ViewData["Description"] = description;
    }

    public sealed record ToolCatalogItem(string Name, string Description, string JsonSchema);
}
