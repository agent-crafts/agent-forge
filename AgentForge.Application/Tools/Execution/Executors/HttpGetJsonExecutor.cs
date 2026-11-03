using System.Text.Json;
using AgentForge.Application.Tools.Registry.Http;

namespace AgentForge.Application.Tools.Execution.Http;

/// <summary>Execution adapter for configured read-only JSON GET tools.</summary>
public sealed class HttpGetJsonExecutor(HttpApiTools tools) : IToolExecutor
{
    public string ToolName => "http.get_json";

    public Task<ToolExecutionResult> ExecuteAsync(JsonElement input, CancellationToken cancellationToken)
        => Task.FromResult(ToolExecutionResult.Fail("missing_configuration", "Configure the API URL on the tool."));

    public async Task<ToolExecutionResult> ExecuteAsync(JsonElement input, JsonElement configuration, CancellationToken cancellationToken)
    {
        if (configuration.ValueKind != JsonValueKind.Object ||
            !configuration.TryGetProperty("url", out var value) || value.ValueKind != JsonValueKind.String)
            return ToolExecutionResult.Fail("invalid_configuration", "Configure a public HTTPS API URL.");

        if (!HttpUrlTemplate.TryResolve(value.GetString(), input, out var url))
            return ToolExecutionResult.Fail("invalid_input", "Provide scalar values for every URL placeholder (for example {id}).");

        var result = await tools.GetJsonAsync(url!, cancellationToken);
        return result.Success
            ? ToolExecutionResult.Ok(result.Data)
            : ToolExecutionResult.Fail(result.Error?.Code ?? "execution_failed", result.Error?.Message ?? "Execution failed.");
    }
}
