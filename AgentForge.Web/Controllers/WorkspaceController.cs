using AgentForge.Application.Tools.Queries;
using AgentForge.Core.Domains;
using AgentForge.Core.Utilities;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

/// <summary>Navigation-only workspace pages; no agent runtime is wired yet.</summary>
[Route("")]
public sealed class WorkspaceController(ISender sender) : Controller
{
    private static readonly Dictionary<string, (string Title, string Description)> Pages = new(StringComparer.OrdinalIgnoreCase)
    {
        ["projects"] = ("Projects", "Organize software projects and their development lifecycle."),
        ["specifications"] = ("Specifications", "Draft and review technical requirements and specifications."),
        ["workflows"] = ("Workflows", "Design and orchestrate development workflows."),
        ["tasks"] = ("Tasks", "Track work items across your development lifecycle."),
        ["reviews"] = ("Reviews & Approvals", "Review changes and manage human approval checkpoints."),
        ["pipelines"] = ("Pipelines", "Track build, test, and delivery pipelines."),
        ["agents"] = ("Agents", "Create, configure, and test your AI agents."),
        ["knowledge"] = ("Knowledge", "Connect documents and knowledge sources to your agents."),
        ["runs"] = ("Runs", "Inspect agent executions, steps, and tool calls.")
    };

    [HttpGet("tools")]
    public async Task<IActionResult> Tools(CancellationToken cancellationToken)
    {
        var page = await sender.Send(new GetToolsPageQuery(new PagedQuery<Tool>(1, 100, t => t.IsActive)), cancellationToken);
        ViewData["Title"] = "Tools";
        ViewData["Section"] = "tools";
        var items = page.Items.Select(tool => new ToolsController.ToolCatalogItem(
            tool.Name, tool.Description, tool.JsonSchema, tool.DisplayName,
            tool.Name == "get_github_issue" ? "github" : "wrench",
            tool.Category.ToString().ToLowerInvariant(), tool.Id, tool.ImageUrl, tool.Kind.ToString().ToLowerInvariant())).ToList();
        return View("~/Views/Tools/Index.cshtml", items);
    }

    [HttpGet("tools/{name}")]
    public async Task<IActionResult> ToolDetails(string name, CancellationToken cancellationToken)
    {
        var page = await sender.Send(new GetToolsPageQuery(new PagedQuery<Tool>(1, 100, t => t.IsActive && t.Name == name)), cancellationToken);
        var tool = page.Items.FirstOrDefault();
        if (tool is null) return NotFound();
        ViewData["Title"] = tool.DisplayName;
        ViewData["ParentTitle"] = "Tools";
        ViewData["ParentController"] = "Workspace";
        ViewData["ParentAction"] = nameof(Tools);
        ViewData["Section"] = "tools";
        var item = new ToolsController.ToolCatalogItem(
            tool.Name, tool.Description, tool.JsonSchema, tool.DisplayName,
            tool.Name == "get_github_issue" ? "github" : "wrench",
            tool.Category.ToString().ToLowerInvariant(), tool.Id, tool.ImageUrl, tool.Kind.ToString().ToLowerInvariant());
        return View("~/Views/Tools/Details.cshtml", item);
    }

    [HttpGet("{section:regex(^(projects|specifications|workflows|tasks|reviews|pipelines|agents|knowledge|runs)$)}")]
    public IActionResult Index(string section)
    {
        if (!Pages.TryGetValue(section, out var page)) return NotFound();
        ViewData["Title"] = page.Title;
        ViewData["Section"] = section.ToLowerInvariant();
        ViewData["Description"] = page.Description;
        return View("~/Views/Workspace/Index.cshtml");
    }
}
