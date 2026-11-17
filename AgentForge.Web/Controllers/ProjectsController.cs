using AgentForge.Application.Projects.Commands;
using AgentForge.Application.Projects.Queries;
using AgentForge.Core.Domains;
using AgentForge.Core.Utilities;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] CreateProjectCommand command, CancellationToken ct)
    {
        try
        {
            var id = await sender.Send(command, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var project = await sender.Send(new GetProjectByIdQuery(id), ct);
        return project is null || !project.IsActive ? NotFound() : Ok(project);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100) return BadRequest("Invalid pagination parameters.");
        return Ok(await sender.Send(new GetProjectsPageQuery(new PagedQuery<Project>(pageNumber, pageSize, p => p.IsActive, p => p.OrderBy(x => x.Name))), ct));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProjectDetailsRequest request, CancellationToken ct)
    {
        try
        {
            return await sender.Send(new UpdateProjectCommand(id, request.Name, request.Description, request.Icon), ct)
                ? NoContent() : NotFound();
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => await sender.Send(new DeleteProjectCommand(id), ct) ? NoContent() : NotFound();

    public sealed record ProjectDetailsRequest(string Name, string? Description, string? Icon = null);

    // Compatibility model for Razor views; API actions return domain models.
    public sealed record ProjectCard(string Id, string Name, string? Description, string Icon,
        DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
    {
        public static ProjectCard From(Project project) => new(
            project.Id.ToString(), project.Name, project.Description,
            project.Icon.StartsWith("projects/", StringComparison.Ordinal) ? "/api/files/" + project.Icon : string.Empty,
            project.CreatedAt, project.UpdatedAt);
    }
}
