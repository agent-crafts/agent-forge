using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

/// <summary>Navigation-only workspace pages; no agent runtime is wired yet.</summary>
[Route("")]
public sealed class WorkspaceController : Controller
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
        ["tools"] = ("Tools", "Manage the capabilities your agents can use."),
        ["knowledge"] = ("Knowledge", "Connect documents and knowledge sources to your agents."),
        ["runs"] = ("Runs", "Inspect agent executions, steps, and tool calls.")
    };

    [HttpGet("{section:regex(^(projects|specifications|workflows|tasks|reviews|pipelines|agents|tools|knowledge|runs)$)}")]
    public IActionResult Index(string section)
    {
        if (!Pages.TryGetValue(section, out var page)) return NotFound();
        ViewData["Title"] = page.Title;
        ViewData["Section"] = section.ToLowerInvariant();
        ViewData["Description"] = page.Description;
        return View("~/Views/Workspace/Index.cshtml");
    }
}
