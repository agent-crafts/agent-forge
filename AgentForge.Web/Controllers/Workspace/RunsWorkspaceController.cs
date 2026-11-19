using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("projects/{id:guid}/runs")]
public sealed class RunsWorkspaceController(ISender sender) : ProjectWorkspaceController(sender)
{
    [HttpGet]
    public Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
        => Render(id, "runs", "~/Views/Workspace/AgentStudio/Runs/Index.cshtml", cancellationToken);
}
