using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("projects/{id:guid}/design")]
public sealed class DesignController(ISender sender) : WorkspacePageController(sender)
{
    [HttpGet]
    public Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
        => Render(id, "design", "~/Views/Workspace/Operations/Design/Index.cshtml", cancellationToken);
}
