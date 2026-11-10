using System.Net.Sockets;
using System.Text.Json;
using AgentForge.Core.Domains;
using AgentForge.Core.Enums;
using Microsoft.Extensions.Logging;

namespace AgentForge.Application.Tools.Execution.Mcp;

public sealed class McpToolExecutionAdapter(McpToolClient client, ILogger<McpToolExecutionAdapter> logger) : IToolExecutionAdapter
{
    public bool CanHandle(Tool tool) =>
        tool.Kind == ToolKind.External &&
        string.Equals(tool.ExecutorKey, "mcp", StringComparison.Ordinal);

    public async Task<ToolExecutionResult> ExecuteAsync(
        Tool tool, JsonElement arguments, CancellationToken cancellationToken)
    {
        if (!CanHandle(tool))
            return ToolExecutionResult.Fail("executor_unavailable", "This adapter only executes MCP tools.");
        if (arguments.ValueKind != JsonValueKind.Object)
            return ToolExecutionResult.Fail("invalid_input", "MCP arguments must be a JSON object.");

        string? endpointHost = null;
        try
        {
            using var configuration = JsonDocument.Parse(tool.ConfigurationJson);
            var config = configuration.RootElement;
            if (!config.TryGetProperty("endpoint", out var endpointValue) ||
                endpointValue.ValueKind != JsonValueKind.String ||
                !Uri.TryCreate(endpointValue.GetString(), UriKind.Absolute, out var endpoint) ||
                !config.TryGetProperty("toolName", out var nameValue) ||
                nameValue.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(nameValue.GetString()))
                return ToolExecutionResult.Fail("invalid_configuration", "MCP endpoint or tool name is missing.");

            endpointHost = endpoint.IdnHost;
            var result = await client.CallAsync(endpoint, nameValue.GetString()!, arguments, cancellationToken);
            if (result.TryGetProperty("isError", out var isError) && isError.ValueKind == JsonValueKind.True)
                return ToolExecutionResult.Fail("mcp_tool_error", "MCP tool reported an execution error.");
            return ToolExecutionResult.Ok(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("MCP execution timed out for tool {ToolId} at {Host}", tool.Id, endpointHost);
            return ToolExecutionResult.Fail("mcp_timeout", "MCP tool request timed out.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning("MCP HTTP request failed for tool {ToolId} at {Host}: {RequestError}, HTTP {Status}, socket {SocketError}",
                tool.Id, endpointHost, ex.HttpRequestError, (int?)ex.StatusCode, (ex.InnerException as SocketException)?.SocketErrorCode);
            var message = ex.StatusCode is { } status
                ? $"The MCP server returned HTTP {(int)status}."
                : "Could not connect to the MCP server.";
            return ToolExecutionResult.Fail("mcp_execution_failed", message);
        }
        catch (Exception ex) when (ex is JsonException or IOException or InvalidDataException or InvalidOperationException)
        {
            logger.LogWarning("MCP execution failed for tool {ToolId} at {Host}: {ErrorType}", tool.Id, endpointHost, ex.GetType().Name);
            return ToolExecutionResult.Fail("mcp_execution_failed", "Could not read or validate the MCP response.");
        }
    }
}
