using AgentForge.Application.Agents.Queries;
using AgentForge.Core.Domains;
using AgentForge.Core.Enums;
using AgentForge.Core.Utilities;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("agents")]
public sealed class AgentsController(ISender sender) : Controller
{
    public sealed record AgentCard(string Id, string Name, string Description, string Category, string Icon, string[] Tags, string SystemInstructions, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, bool IsActive)
    {
        public static AgentCard From(Agent agent) => new(
            agent.Id.ToString(), agent.Name, agent.Description, agent.Category.ToString(),
            IconUrlOrDefault(agent.Icon), agent.Tags.ToArray(), agent.SystemInstructions,
            agent.CreatedAt, agent.UpdatedAt, agent.IsActive);

        private static string IconUrlOrDefault(string icon)
        {
            var ext = Path.GetExtension(icon).ToLowerInvariant();
            var stem = Path.GetFileNameWithoutExtension(icon);
            return stem.Length == 24 && stem.All(Uri.IsHexDigit) && ext is ".png" or ".jpg" or ".webp" or ".gif"
                ? "/uploads/agent-icons/" + icon
                : icon;
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var agent = await sender.Send(new GetAgentByIdQuery(id), cancellationToken);
        if (agent is null || !agent.IsActive) return NotFound();

        ViewData["Title"] = agent.Name;
        ViewData["Section"] = "agents";
        return View(AgentCard.From(agent));
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) return BadRequest("Invalid page number.");

        const int pageSize = 20;
        var result = await sender.Send(new GetAgentsPageQuery(
            new PagedQuery<Agent>(pageNumber, pageSize, agent => agent.IsActive,
                agents => agents.OrderBy(agent => agent.Name))), cancellationToken);

        ViewData["Categories"] = Enum.GetNames<AgentCategory>();
        ViewData["Title"] = "Agents";
        ViewData["Section"] = "agents";
        ViewData["PageNumber"] = result.PageNumber;
        ViewData["TotalPages"] = result.TotalPages;
        return View(result.Items.Select(AgentCard.From).ToArray());
    }
}
