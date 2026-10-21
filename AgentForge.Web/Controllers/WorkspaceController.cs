using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

/// <summary>Navigation-only workspace pages; no agent runtime is wired yet.</summary>
[Route("")]
public sealed class WorkspaceController : Controller
{
    private static readonly Dictionary<string, (string Title, string Description)> Pages = new(StringComparer.OrdinalIgnoreCase)
    {
        ["agents"] = ("Agents", "Create, configure, and test your AI agents."),
        ["tools"] = ("Tools", "Manage the capabilities your agents can use."),
        ["knowledge"] = ("Knowledge", "Connect documents and knowledge sources to your agents."),
        ["runs"] = ("Runs", "Inspect agent executions, steps, and tool calls."),
        ["settings"] = ("Settings", "Configure your local AgentForge workspace.")
    };

    [HttpGet("{section:regex(^(agents|tools|knowledge|runs|settings)$)}")]
    public IActionResult Index(string section)
    {
        if (!Pages.TryGetValue(section, out var page)) return NotFound();
        ViewData["Title"] = page.Title;
        ViewData["Section"] = section.ToLowerInvariant();
        ViewData["Description"] = page.Description;
        return View("~/Views/Workspace/Index.cshtml");
    }
}
