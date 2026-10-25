using AgentForge.Application.Tools.Commands;
using AgentForge.Application.Tools.Queries;
using AgentForge.Core.Domains;
using AgentForge.Core.Utilities;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[ApiController]
[Route("api/tools")]
public sealed class ToolsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateToolCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateToolCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var updated = await sender.Send(command, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await sender.Send(new DeleteToolCommand(id), cancellationToken) ? NoContent() : NotFound();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tool = await sender.Send(new GetToolByIdQuery(id), cancellationToken);
        return tool is null || !tool.IsActive ? NotFound() : Ok(tool);
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
            new GetToolsPageQuery(new PagedQuery<Tool>(pageNumber, pageSize, tool => tool.IsActive)),
            cancellationToken);
        return Ok(result);
    }
    [HttpPost("{id:guid}/execute")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Execute(
        Guid id,
        [FromBody] System.Text.Json.JsonElement input,
        [FromServices] ILogger<ToolsController> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(new ExecuteToolCommand(id, input), cancellationToken);
            if (result.Success) return Ok(result);

            return result.Error?.Code switch
            {
                "tool_not_found" => NotFound(result),
                "executor_unavailable" => Conflict(result),
                "issue_not_found" => NotFound(result),
                _ => BadRequest(result)
            };
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Tool execution provider failed for {ToolId}", id);
            return StatusCode(502, new { success = false, error = new { code = "provider_unavailable", message = "Tool provider is unavailable." } });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(504, new { success = false, error = new { code = "provider_timeout", message = "Tool provider timed out." } });
        }
    }

    // Compatibility model for legacy Razor views; not used by API actions.
    public sealed record ToolCatalogItem(string Name, string Description, string JsonSchema, string DisplayName, string Icon, string Category, Guid Id = default, string? ImageUrl = null, string Kind = "custom");
}
