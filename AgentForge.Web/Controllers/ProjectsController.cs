using AgentForge.Application.Projects.Commands;
using AgentForge.Application.Projects.Queries;
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
        return project is null || project.IsArchived ? NotFound() : Ok(project);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100) return BadRequest("Invalid pagination parameters.");
        return Ok(await sender.Send(new GetProjectsPageQuery(new AgentForge.Core.Utilities.PagedQuery<AgentForge.Core.Domains.Project>(pageNumber, pageSize, p => p.IsActive && !p.IsArchived, p => p.OrderBy(x => x.Name))), ct));
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

    [HttpPost("{id:guid}/archive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
        => await sender.Send(new ArchiveProjectCommand(id), ct) ? NoContent() : NotFound();

    public sealed record ProjectDetailsRequest(string Name, string? Description, string? Icon = null);
}
