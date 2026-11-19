using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("projects/{id:guid}/design")]
public sealed class DesignWorkspaceController(ISender sender) : ProjectWorkspaceController(sender)
{
    [HttpGet]
    public Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
        => Render(id, "design", "~/Views/Workspace/Operations/Design/Index.cshtml", cancellationToken);
}
