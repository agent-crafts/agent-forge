using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("projects/{id:guid}/workflows")]
public sealed class WorkflowsWorkspaceController(ISender sender) : ProjectWorkspaceController(sender)
{
    [HttpGet]
    public Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
        => Render(id, "workflows", "~/Views/Workspace/AgentStudio/Workflows/Index.cshtml", cancellationToken);
}
