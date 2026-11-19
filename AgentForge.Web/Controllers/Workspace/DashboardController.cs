using AgentForge.Application.Projects.Queries;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("projects/{id:guid}")]
public sealed class DashboardController(ISender sender) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
    {
        var project = await sender.Send(new GetProjectByIdQuery(id), cancellationToken);
        if (project is null || !project.IsActive) return NotFound();

        ViewData["Title"] = project.Name;
        ViewData["Section"] = "projects";
        ViewData["ProjectName"] = project.Name;
        ViewData["ProjectId"] = id.ToString();
        return View("~/Views/Workspace/Dashboard/Index.cshtml", ProjectsController.ProjectCard.From(project));
    }
}
