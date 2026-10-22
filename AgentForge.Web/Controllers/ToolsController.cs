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
            .Select(tool => CreateCatalogItem(tool.Name, tool.Description ?? "", tool.JsonSchema.ToString()))
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
        return View(CreateCatalogItem(tool.Name, tool.Description ?? "", tool.JsonSchema.ToString()));
    }

    private void SetPage(string title, string description)
    {
        ViewData["Title"] = title;
        ViewData["Section"] = "tools";
        ViewData["Description"] = description;
    }

    // Presentation metadata is keyed by the stable tool name; execution identity stays unchanged.
    private static readonly IReadOnlyDictionary<string, (string DisplayName, string Icon, string Category)> ToolPresentation =
        new Dictionary<string, (string, string, string)>(StringComparer.OrdinalIgnoreCase)
        {
            ["get_github_issue"] = ("Get GitHub Issue", "github", "github")
        };

    private static ToolCatalogItem CreateCatalogItem(string name, string description, string jsonSchema)
    {
        if (ToolPresentation.TryGetValue(name, out var presentation))
            return new ToolCatalogItem(name, description, jsonSchema, presentation.DisplayName, presentation.Icon, presentation.Category);

        var displayName = string.Join(" ", name.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
        return new ToolCatalogItem(name, description, jsonSchema, displayName, "tool", "other");
    }

    public sealed record ToolCatalogItem(
        string Name, string Description, string JsonSchema, string DisplayName, string Icon, string Category);
}
