using System.Net;
using System.Text.Json;
using AgentForge.Core.Clients.Http;

namespace AgentForge.Application.Tools.Registers.Http;

/// <summary>Application-owned read-only API operation; delegates HTTP access to the client.</summary>
public sealed class HttpApiTools(IHttpApiClient client)
{
    public async Task<ToolResult<JsonElement>> GetJsonAsync(Uri url, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await client.GetJsonAsync(url, cancellationToken);
            return ToolResult<JsonElement>.Ok(result);
        }
        catch (ArgumentException)
        {
            return ToolResult<JsonElement>.Fail("invalid_configuration", "Only approved HTTPS API URLs are allowed.");
        }
        catch (InvalidDataException)
        {
            return ToolResult<JsonElement>.Fail("invalid_response", "API response is not supported JSON or exceeds 1 MiB.");
        }
        catch (JsonException)
        {
            return ToolResult<JsonElement>.Fail("invalid_response", "API returned invalid JSON.");
        }
        catch (HttpRequestException)
        {
            return ToolResult<JsonElement>.Fail("upstream_error", "Could not retrieve JSON from the configured API.");
        }
    }
}
