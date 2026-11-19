using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("projects/{id:guid}/specifications")]
public sealed class SpecificationsWorkspaceController(ISender sender) : ProjectWorkspaceController(sender)
{
    [HttpGet]
    public Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
        => Render(id, "specifications", "~/Views/Workspace/Specifications/Index.cshtml", cancellationToken);
}
