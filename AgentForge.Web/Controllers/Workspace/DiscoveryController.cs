using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("projects/{id:guid}/discovery")]
public sealed class DiscoveryController(ISender sender) : WorkspacePageController(sender)
{
    [HttpGet]
    public Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
        => Render(id, "discovery", "~/Views/Workspace/Operations/Discovery/Index.cshtml", cancellationToken);
}
