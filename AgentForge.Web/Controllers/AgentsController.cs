using AgentForge.Application.Agents.Commands;
using AgentForge.Application.Agents.Queries;
using AgentForge.Core.Repositories;
using AgentForge.Core.Clients;
using AgentForge.Core.Domains;
using AgentForge.Core.Utilities;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[ApiController]
[Route("api/agents")]
public sealed class AgentsController(ISender sender, ILlmClient llmClient, ILogger<AgentsController> logger) : ControllerBase
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

    public sealed record PlaygroundChatRequest(string? Message);

    [HttpPost("{id:guid}/playground/chat")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaygroundChat(Guid id, [FromBody] PlaygroundChatRequest? request, CancellationToken cancellationToken)
    {
        var message = request?.Message?.Trim();
        if (string.IsNullOrWhiteSpace(message) || message.Length > 8000)
            return BadRequest(new { error = "Message must contain 1–8000 characters." });

        var agent = await sender.Send(new GetAgentByIdQuery(id), cancellationToken);
        if (agent is null || !agent.IsActive) return NotFound();

        var messages = new List<LlmMessage>();
        if (!string.IsNullOrWhiteSpace(agent.SystemInstructions))
            messages.Add(new SystemMessage(agent.SystemInstructions));
        messages.Add(new UserMessage(message));

        try
        {
            // Deliberately text-only: tool execution and session history belong to the Agent Runtime.
            var response = await llmClient.ChatAsync(new LlmChatRequest(messages), cancellationToken);
            if (response.ToolCalls.Count > 0 || string.IsNullOrWhiteSpace(response.Content))
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "LLM did not return a text response." });
            return Ok(new { response = response.Content });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Playground LLM call failed for agent {AgentId}", id);
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "LLM request failed. Check provider configuration and server logs." });
        }
    }

    [HttpGet("{id:guid}/tools")]
    public async Task<IActionResult> GetTools(Guid id, CancellationToken cancellationToken)
    {
        var agent = await sender.Send(new GetAgentByIdQuery(id), cancellationToken);
        if (agent is null || !agent.IsActive) return NotFound();
        return Ok(await sender.Send(new GetAgentToolIdsQuery(id), cancellationToken));
    }

    [HttpPut("{id:guid}/tools")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetTools(Guid id, [FromBody] SetToolsRequest request, CancellationToken cancellationToken)
    {
        if (request?.ToolIds is null) return BadRequest(new { error = "Tool IDs are required." });
        try
        {
            return await sender.Send(new SetAgentToolsCommand(id, request.ToolIds), cancellationToken)
                ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public sealed record SetToolsRequest(Guid[] ToolIds);

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
