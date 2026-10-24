using AgentForge.Application.Tools;
using AgentForge.Application.Tools.GitHub;
using AgentForge.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

/// <summary>Read-only catalog. Tool execution is deliberately not exposed by this controller.</summary>
[Route("tools")]
public sealed class ToolsController(IToolRegistry registry, GitHubTools githubTools, ILogger<ToolsController> logger) : Controller
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

        var item = CreateCatalogItem(tool.Name, tool.Description ?? "", tool.JsonSchema.ToString());
        SetPage(item.DisplayName, "Review the tool's description and input contract.");
        ViewData["ParentTitle"] = "Tools";
        ViewData["ParentController"] = "Tools";
        return View(item);
    }

    // Narrow, read-only demo execution. No arbitrary tool invocation is exposed.
    [HttpPost("get_github_issue/try")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TryGitHubIssue([FromForm] int issueNumber, CancellationToken cancellationToken)
    {
        if (issueNumber <= 0)
            return BadRequest(new { success = false, error = "Issue number must be a positive integer." });

        try
        {
            var result = await githubTools.GetGitHubIssueAsync(issueNumber, cancellationToken);
            return Json(result);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "GitHub issue lookup failed for issue {IssueNumber}", issueNumber);
            return StatusCode(502, new { success = false, error = "GitHub could not complete the request. Please try again." });
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(504, new { success = false, error = "GitHub request timed out." });
        }
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
            ["get_github_issue"] = ("Get GitHub Issue", "github", nameof(ToolCategory.Integration))
        };

    private static ToolCatalogItem CreateCatalogItem(string name, string description, string jsonSchema)
    {
        if (ToolPresentation.TryGetValue(name, out var presentation))
            return new ToolCatalogItem(name, description, jsonSchema, presentation.DisplayName, presentation.Icon, presentation.Category.ToLowerInvariant());

        var displayName = string.Join(" ", name.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
        return new ToolCatalogItem(name, description, jsonSchema, displayName, "tool", nameof(ToolCategory.General).ToLowerInvariant());
    }

    public sealed record ToolCatalogItem(
        string Name, string Description, string JsonSchema, string DisplayName, string Icon, string Category);
}
