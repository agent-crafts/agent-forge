using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("projects/{id:guid}/board")]
public sealed class BoardController(ISender sender) : DashboardController(sender)
{
    [HttpGet]
    public Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
        => Render(id, "board", "~/Views/Workspace/Board/Index.cshtml", cancellationToken);
}
