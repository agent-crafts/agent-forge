using AgentForge.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

/// <summary>Static project UI preview; no writes, persistence or external connections.</summary>
[Route("projects")]
public sealed class ProjectsController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Projects";
        ViewData["Section"] = "projects";
        ViewData["Description"] = "Your AI engineering workspaces.";
        return View(ProjectPreviewData.Projects);
    }

    [HttpGet("{slug}")]
    public IActionResult Profile(string slug, [FromQuery] string tab = "general")
    {
        var project = ProjectPreviewData.Find(slug);
        if (project is null) return NotFound();

        var allowed = new[] { "general", "integrations", "knowledge", "agents", "team", "permissions", "advanced" };
        if (!allowed.Contains(tab, StringComparer.OrdinalIgnoreCase)) return NotFound();

        ViewData["Title"] = project.Name;
        ViewData["Section"] = "projects";
        ViewData["Description"] = "Project profile and configuration.";
        ViewData["Tab"] = tab.ToLowerInvariant();
        return View(project);
    }
}
