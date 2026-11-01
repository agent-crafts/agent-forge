using AgentForge.Application.Agents.Commands;
using AgentForge.Application.Agents.Queries;
using AgentForge.Core.Domains;
using AgentForge.Core.Utilities;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[ApiController]
[Route("api/agents")]
public sealed class AgentsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [FromBody] CreateAgentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAgentCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        try
        {
            var updated = await sender.Send(command, cancellationToken);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => await sender.Send(new DeactivateAgentCommand(id), cancellationToken) ? NoContent() : NotFound();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var agent = await sender.Send(new GetAgentByIdQuery(id), cancellationToken);
        return agent is null || !agent.IsActive ? NotFound() : Ok(agent);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100)
            return BadRequest("Invalid pagination parameters.");

        var result = await sender.Send(
            new GetAgentsPageQuery(new PagedQuery<Agent>(pageNumber, pageSize, agent => agent.IsActive,
                agents => agents.OrderBy(agent => agent.Name))),
            cancellationToken);
        return Ok(result);
    }

    // Compatibility model for Razor views; not used by API actions.
    public sealed record AgentCard(string Id, string Name, string Description, string Category, string Icon, string[] Tags, string SystemInstructions, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, bool IsActive)
    {
        public static AgentCard From(Agent agent) => new(
            agent.Id.ToString(), agent.Name, agent.Description, agent.Category.ToString(),
            IconUrlOrDefault(agent.Icon), agent.Tags.ToArray(), agent.SystemInstructions,
            agent.CreatedAt, agent.UpdatedAt, agent.IsActive);

        private static string IconUrlOrDefault(string icon)
        {
            if (icon.StartsWith("agents/", StringComparison.Ordinal)) return "/api/files/" + icon;
            var ext = Path.GetExtension(icon).ToLowerInvariant();
            var stem = Path.GetFileNameWithoutExtension(icon);
            return stem.Length == 24 && stem.All(Uri.IsHexDigit) && ext is ".png" or ".jpg" or ".webp" or ".gif"
                ? "/api/files/" + icon
                : icon;
        }
    }
}
