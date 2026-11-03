using System.Net.Http;
using System.Text.Json;
using AgentForge.Core.Domains;
using AgentForge.Core.Enums;

namespace AgentForge.Application.Tools.Execution.Mcp;

public sealed class McpToolExecutionAdapter(McpToolClient client) : IToolExecutionAdapter
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

            var result = await client.CallAsync(endpoint, nameValue.GetString()!, arguments, cancellationToken);
            if (result.TryGetProperty("isError", out var isError) && isError.ValueKind == JsonValueKind.True)
                return ToolExecutionResult.Fail("mcp_tool_error", "MCP tool returned an error: " + result.GetRawText());
            return ToolExecutionResult.Ok(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return ToolExecutionResult.Fail("mcp_timeout", "MCP tool request timed out.");
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException or HttpRequestException or InvalidOperationException)
        {
            return ToolExecutionResult.Fail("mcp_execution_failed", "MCP tool execution failed: " + ex.Message);
        }
    }
}
