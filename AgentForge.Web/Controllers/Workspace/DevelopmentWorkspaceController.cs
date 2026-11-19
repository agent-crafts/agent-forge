using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("projects/{id:guid}/development")]
public sealed class DevelopmentWorkspaceController(ISender sender) : ProjectWorkspaceController(sender)
{
    [HttpGet]
    public Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
        => Render(id, "development", "~/Views/Workspace/Operations/Development/Index.cshtml", cancellationToken);
}
