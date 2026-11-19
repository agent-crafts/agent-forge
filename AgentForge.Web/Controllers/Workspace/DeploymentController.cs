using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("projects/{id:guid}/deployment")]
public sealed class DeploymentController(ISender sender) : DashboardController(sender)
{
    [HttpGet]
    public Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
        => Render(id, "deployment", "~/Views/Workspace/Operations/Deployment/Index.cshtml", cancellationToken);
}
