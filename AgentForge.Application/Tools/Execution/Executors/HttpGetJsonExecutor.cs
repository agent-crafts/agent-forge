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
            !configuration.TryGetProperty("url", out var value) || value.ValueKind != JsonValueKind.String ||
            !Uri.TryCreate(value.GetString(), UriKind.Absolute, out var url) ||
            url.Scheme != Uri.UriSchemeHttps || url.UserInfo.Length != 0 || url.Port != 443 ||
            !string.Equals(url.Host, "fakestoreapi.com", StringComparison.OrdinalIgnoreCase))
            return ToolExecutionResult.Fail("invalid_configuration", "Only HTTPS URLs on fakestoreapi.com are approved.");

        if (input.ValueKind != JsonValueKind.Object || input.EnumerateObject().Any())
            return ToolExecutionResult.Fail("invalid_input", "This configured GET tool takes no runtime arguments.");

        var result = await tools.GetJsonAsync(url, cancellationToken);
        return result.Success
            ? ToolExecutionResult.Ok(result.Data)
            : ToolExecutionResult.Fail(result.Error?.Code ?? "execution_failed", result.Error?.Message ?? "Execution failed.");
    }
}
