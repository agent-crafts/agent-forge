using System.Text.Json;
using System.Text.RegularExpressions;
using AgentForge.Application.Tools.Commands;
using AgentForge.Core.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgentForge.Web.Services;

namespace AgentForge.Web.Controllers;

[ApiController]
[Route("api/tools/mcp")]
public sealed class McpDiscoveryController(ILogger<McpDiscoveryController> logger, ISender sender, McpDiscoveryService discoveryService) : ControllerBase
{
    public sealed record DiscoverRequest(string Endpoint);

    public sealed record ImportRequest(string Endpoint, string[] ToolNames, ToolCategory Category = ToolCategory.Integration);

    [HttpPost("import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import([FromBody] ImportRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Category))
            return BadRequest(new { error = "Select a valid tool category." });

        if (request.ToolNames is null || request.ToolNames.Length is < 1 or > 200 ||
            request.ToolNames.Any(string.IsNullOrWhiteSpace) ||
            request.ToolNames.Distinct(StringComparer.Ordinal).Count() != request.ToolNames.Length)
            return BadRequest(new { error = "Select between 1 and 200 distinct tools." });

        // Re-discover on the server; never trust client-supplied descriptions or schemas.
        var discovery = await discoveryService.DiscoverAsync(request.Endpoint, cancellationToken);
        if (discovery.Tools is null)
            return StatusCode(discovery.StatusCode, discovery.Error);

        var available = discovery.Tools
            .GroupBy(tool => tool.name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        if (request.ToolNames.Any(name => !available.ContainsKey(name)))
            return BadRequest(new { error = "Some selected tools are no longer available. Discover again." });

        var items = new List<ImportMcpToolItem>();
        foreach (var name in request.ToolNames)
        {
            var tool = available[name];
            var normalized = Regex.Replace(name.ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
            var persistedName = "mcp_" + normalized;
            if (normalized.Length == 0 || persistedName.Length > 155)
                return BadRequest(new { error = "An MCP tool name cannot be imported: " + name });
            var description = tool.description;
            var schema = tool.inputSchema.GetRawText();
            var configuration = JsonSerializer.Serialize(new { endpoint = request.Endpoint, toolName = name });
            var displayName = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.Replace("_", " ").ToLowerInvariant());
            items.Add(new ImportMcpToolItem(persistedName, displayName, description, schema, configuration));
        }

        if (items.Select(item => item.Name).Distinct(StringComparer.Ordinal).Count() != items.Count)
            return BadRequest(new { error = "Selected MCP tool names conflict after normalization. Import the conflicting tools separately." });

        try
        {
            var count = await sender.Send(new ImportMcpToolsCommand(items, request.Category), cancellationToken);
            return Ok(new { imported = count });
        }
        catch (InvalidOperationException)
        {
            return Conflict(new { error = "One or more tools already exist. Remove duplicates before importing." });
        }
        catch (ArgumentException)
        {
            return BadRequest(new { error = "One or more selected tools have invalid metadata." });
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "MCP batch persistence failed for {ToolCount} tools", items.Count);
            return Conflict(new { error = "Database rejected the import. A tool name may already exist, including an inactive tool. Check server logs." });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "MCP metadata import failed for {ToolCount} tools", items.Count);
            return StatusCode(500, new { error = "Could not persist MCP tools. Check server logs." });
        }
    }

    [HttpPost("discover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Discover([FromBody] DiscoverRequest request, CancellationToken cancellationToken)
    {
        var result = await discoveryService.DiscoverAsync(request.Endpoint, cancellationToken);
        return result.Tools is not null
            ? Ok(new { tools = result.Tools })
            : StatusCode(result.StatusCode, result.Error);
    }
}
