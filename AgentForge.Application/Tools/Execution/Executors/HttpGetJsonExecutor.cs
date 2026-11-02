using System.Net;
using System.Text.Json;

namespace AgentForge.Application.Tools.Execution.Http;

/// <summary>Read-only JSON GET for explicitly approved public API hosts.</summary>
public sealed class HttpGetJsonExecutor(HttpClient client) : IToolExecutor
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
            return ToolExecutionResult.Fail("invalid_configuration", "Only HTTPS URLs on fakestoreapi.com are approved for this executor.");

        if (input.ValueKind != JsonValueKind.Object || input.EnumerateObject().Any())
            return ToolExecutionResult.Fail("invalid_input", "This configured GET tool takes no runtime arguments.");

        try
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return ToolExecutionResult.Fail("upstream_error", $"API returned HTTP {(int)response.StatusCode}.");
            if (response.Content.Headers.ContentType?.MediaType is not ("application/json" or "application/problem+json"))
                return ToolExecutionResult.Fail("invalid_response", "API did not return JSON.");
            if (response.Content.Headers.ContentLength is > 1_048_576)
                return ToolExecutionResult.Fail("response_too_large", "Response exceeds 1 MiB.");
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var buffer = new MemoryStream();
            var chunk = new byte[8192];
            int count;
            while ((count = await stream.ReadAsync(chunk, cancellationToken)) > 0)
            {
                if (buffer.Length + count > 1_048_576)
                    return ToolExecutionResult.Fail("response_too_large", "Response exceeds 1 MiB.");
                buffer.Write(chunk, 0, count);
            }
            buffer.Position = 0;
            using var json = await JsonDocument.ParseAsync(buffer, cancellationToken: cancellationToken);
            return ToolExecutionResult.Ok(json.RootElement.Clone());
        }
        catch (JsonException)
        {
            return ToolExecutionResult.Fail("invalid_response", "API returned invalid JSON.");
        }
        catch (HttpRequestException)
        {
            return ToolExecutionResult.Fail("upstream_error", "Could not reach the configured API.");
        }
    }
}
