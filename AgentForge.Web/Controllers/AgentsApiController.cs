using AgentForge.Application.Agents.Commands;
using AgentForge.Application.Agents.Queries;
using AgentForge.Core.Domains;
using AgentForge.Core.Enums;
using AgentForge.Core.Utilities;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

/// <summary>HTTP boundary for persisted Agent definitions; execution is out of scope.</summary>
[ApiController]
[Route("api/agents")]
public sealed class AgentsApiController(ISender sender) : ControllerBase
{
    public sealed record SaveAgentRequest(
        string Name, string Description, AgentCategory Category,
        string SystemInstructions, string? Icon = null, string[]? Tags = null);

    public sealed record AgentResponse(
        Guid Id, string Name, string Description, AgentCategory Category,
        string Icon, string[] Tags, string SystemInstructions,
        bool IsActive, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
    {
        public static AgentResponse From(Agent agent) =>
            new(agent.Id, agent.Name, agent.Description, agent.Category,
                agent.Icon, agent.Tags.ToArray(), agent.SystemInstructions,
                agent.IsActive, agent.CreatedAt, agent.UpdatedAt);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] SaveAgentRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest();
        try
        {
            var id = await sender.Send(new CreateAgentCommand(request.Name, request.Description,
                request.Category, request.SystemInstructions, request.Icon, request.Tags), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveAgentRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest();
        try
        {
            var updated = await sender.Send(new UpdateAgentCommand(id, request.Name, request.Description,
                request.Category, request.SystemInstructions, request.Icon, request.Tags), cancellationToken);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
        => await sender.Send(new DeactivateAgentCommand(id), cancellationToken) ? NoContent() : NotFound();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var agent = await sender.Send(new GetAgentByIdQuery(id), cancellationToken);
        return agent is null || !agent.IsActive ? NotFound() : Ok(AgentResponse.From(agent));
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100)
            return BadRequest("Invalid pagination parameters.");

        var page = await sender.Send(new GetAgentsPageQuery(
            new PagedQuery<Agent>(pageNumber, pageSize, agent => agent.IsActive,
                agents => agents.OrderBy(agent => agent.Name))), cancellationToken);
        return Ok(new PagedResult<AgentResponse>(
            page.Items.Select(AgentResponse.From).ToArray(),
            page.PageNumber, page.PageSize, page.TotalCount));
    }
}
