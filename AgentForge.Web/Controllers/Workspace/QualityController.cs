using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("projects/{id:guid}/quality")]
public sealed class QualityController(ISender sender) : WorkspacePageController(sender)
{
    [HttpGet]
    public Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
        => Render(id, "quality", "~/Views/Workspace/Operations/Quality/Index.cshtml", cancellationToken);
}
