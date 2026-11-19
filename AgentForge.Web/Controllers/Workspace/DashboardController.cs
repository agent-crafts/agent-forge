using AgentForge.Application.Projects.Queries;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

/// <summary>Shared project lookup for workspace UI routes. Demo views do not execute agents.</summary>
public abstract class DashboardController(ISender sender) : Controller
{
    protected async Task<IActionResult> Render(Guid id, string section, string viewPath, CancellationToken ct)
    {
        var project = await sender.Send(new GetProjectByIdQuery(id), ct);
        if (project is null || !project.IsActive) return NotFound();
        ViewData["Title"] = char.ToUpperInvariant(section[0]) + section[1..];
        ViewData["Section"] = "project-" + section;
        ViewData["ProjectName"] = project.Name;
        ViewData["ProjectId"] = id.ToString();
        ViewData["WorkspaceStage"] = section;
        return View(viewPath);
    }
}
