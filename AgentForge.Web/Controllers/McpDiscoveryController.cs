using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentForge.Application.Tools.Commands;
using Mediator;
using AgentForge.Core.Clients.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[ApiController]
[Route("api/tools/mcp")]
public sealed class McpDiscoveryController(ILogger<McpDiscoveryController> logger, ISender sender) : ControllerBase
{
    public sealed record DiscoverRequest(string Endpoint);

    public sealed record ImportRequest(string Endpoint, string[] ToolNames);

    [HttpPost("import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import([FromBody] ImportRequest request, CancellationToken cancellationToken)
    {
        if (request.ToolNames is null || request.ToolNames.Length is < 1 or > 200 ||
            request.ToolNames.Any(string.IsNullOrWhiteSpace) ||
            request.ToolNames.Distinct(StringComparer.Ordinal).Count() != request.ToolNames.Length)
            return BadRequest(new { error = "Select between 1 and 200 distinct tools." });

        // Re-discover on the server; never trust client-supplied descriptions or schemas.
        var discovery = await Discover(new DiscoverRequest(request.Endpoint), cancellationToken);
        if (discovery is not OkObjectResult ok)
            return discovery;

        using var payload = JsonDocument.Parse(JsonSerializer.Serialize(ok.Value));
        var available = payload.RootElement.GetProperty("tools").EnumerateArray()
            .Where(item => item.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
            .GroupBy(item => item.GetProperty("name").GetString()!, StringComparer.Ordinal)
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
            var description = tool.GetProperty("description").GetString() ?? "";
            var schema = tool.GetProperty("inputSchema").GetRawText();
            var configuration = JsonSerializer.Serialize(new { endpoint = request.Endpoint, toolName = name });
            items.Add(new ImportMcpToolItem(persistedName, name, description, schema, configuration));
        }

        try
        {
            var count = await sender.Send(new ImportMcpToolsCommand(items), cancellationToken);
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
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "MCP metadata import failed");
            return StatusCode(500, new { error = "Could not persist MCP tools. Check server logs." });
        }
    }

    [HttpPost("discover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Discover([FromBody] DiscoverRequest request, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.Endpoint, UriKind.Absolute, out var endpoint) || !PublicHttpUrlPolicy.IsAllowed(endpoint))
            return BadRequest(new { error = "Enter a public HTTPS MCP endpoint." });

        using var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false, UseCookies = false, UseProxy = false,
            ConnectCallback = async (context, ct) =>
            {
                var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, ct);
                var address = addresses.FirstOrDefault(PublicHttpUrlPolicy.IsPublicAddress)
                    ?? throw new HttpRequestException("No public destination IP.");
                var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    await socket.ConnectAsync(new IPEndPoint(address, context.DnsEndPoint.Port), ct);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch { socket.Dispose(); throw; }
            }
        };
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(12) };
        try
        {
            string negotiatedVersion = "2025-03-26";
            using var init = await SendAsync("initialize", new
            {
                protocolVersion = "2025-03-26", capabilities = new { },
                clientInfo = new { name = "AgentForge", version = "0.1" }
            }, 1);
            if (init.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                return StatusCode(401, new { error = "Authentication-required MCP discovery is not supported yet." });
            if (!init.IsSuccessStatusCode) return BadRequest(new { error = "MCP initialize failed.", status = (int)init.StatusCode });
            using (var document = await ParseAsync(init, cancellationToken))
            {
                if (!document.RootElement.TryGetProperty("result", out var result) ||
                    !result.TryGetProperty("protocolVersion", out var version) || version.ValueKind != JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(version.GetString()))
                    return BadRequest(new { error = "Invalid MCP initialize response." });
                negotiatedVersion = version.GetString()!;
            }

            var session = init.Headers.TryGetValues("Mcp-Session-Id", out var values) ? values.FirstOrDefault() : null;
            using (var message = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                message.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
                message.Headers.TryAddWithoutValidation("MCP-Protocol-Version", negotiatedVersion);
                if (session is not null) message.Headers.TryAddWithoutValidation("Mcp-Session-Id", session);
                message.Content = new StringContent("{\"jsonrpc\":\"2.0\",\"method\":\"notifications/initialized\"}", Encoding.UTF8, "application/json");
                using var notification = await client.SendAsync(message, cancellationToken);
                if (!notification.IsSuccessStatusCode)
                {
                    logger.LogWarning("MCP initialized notification rejected by {Host} with HTTP {Status}", endpoint.Host, (int)notification.StatusCode);
                    return BadRequest(new { error = "MCP initialization notification failed.", status = (int)notification.StatusCode });
                }
            }

            var discovered = new List<object>();
            string? cursor = null;
            for (var page = 0; page < 10; page++)
            {
                using var response = await SendAsync("tools/list", cursor is null ? new { } : (object)new { cursor }, page + 2, session);
                if (!response.IsSuccessStatusCode) return BadRequest(new { error = "MCP tools/list failed.", status = (int)response.StatusCode });
                using var document = await ParseAsync(response, cancellationToken);
                if (!document.RootElement.TryGetProperty("result", out var result) ||
                    !result.TryGetProperty("tools", out var tools) || tools.ValueKind != JsonValueKind.Array)
                    return BadRequest(new { error = "Invalid MCP tools/list response." });
                foreach (var tool in tools.EnumerateArray())
                {
                    if (discovered.Count >= 200) return BadRequest(new { error = "Tool discovery exceeded 200 tools." });
                    if (!tool.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String) continue;
                    var description = tool.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String ? desc.GetString() : "";
                    var schema = tool.TryGetProperty("inputSchema", out var input) && input.ValueKind == JsonValueKind.Object
                        ? input.Clone() : JsonSerializer.SerializeToElement(new { type = "object" });
                    discovered.Add(new { name = name.GetString(), description, inputSchema = schema });
                }
                cursor = result.TryGetProperty("nextCursor", out var next) && next.ValueKind == JsonValueKind.String ? next.GetString() : null;
                if (string.IsNullOrEmpty(cursor)) return Ok(new { tools = discovered });
            }
            return BadRequest(new { error = "Tool discovery exceeded 10 pages." });

            async Task<HttpResponseMessage> SendAsync(string method, object parameters, int id, string? sessionId = null)
            {
                using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
                message.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
                message.Headers.TryAddWithoutValidation("MCP-Protocol-Version", negotiatedVersion);
                if (sessionId is not null) message.Headers.TryAddWithoutValidation("Mcp-Session-Id", sessionId);
                message.Content = new StringContent(JsonSerializer.Serialize(new { jsonrpc = "2.0", id, method, @params = parameters }), Encoding.UTF8, "application/json");
                return await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidDataException)
        {
            logger.LogWarning("MCP discovery failed for host {Host}: {ErrorType}", endpoint.Host, ex.GetType().Name);
            return StatusCode(502, new { error = "MCP discovery failed. Check endpoint and server connectivity." });
        }
    }

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int count;
        while ((count = await stream.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + count > 1_048_576) throw new InvalidDataException("Response exceeds 1 MiB.");
            buffer.Write(chunk, 0, count);
        }
        var content = Encoding.UTF8.GetString(buffer.ToArray());
        if (response.Content.Headers.ContentType?.MediaType == "text/event-stream")
        {
            content = content.Split('\n').Where(line => line.StartsWith("data:", StringComparison.Ordinal))
                .Select(line => line[5..].Trim()).FirstOrDefault(line => line.StartsWith('{'))
                ?? throw new InvalidDataException("Missing JSON-RPC SSE event.");
        }
        return JsonDocument.Parse(content);
    }
}
