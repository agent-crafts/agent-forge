using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using AgentForge.Core.Clients.Http;

namespace AgentForge.Application.Tools.Execution.Mcp;

/// <summary>Executes a single Streamable HTTP MCP tool call against a public HTTPS endpoint.</summary>
public sealed class McpToolClient
{
    public async Task<JsonElement> CallAsync(Uri endpoint, string toolName, JsonElement arguments, CancellationToken cancellationToken)
    {
        if (!PublicHttpUrlPolicy.IsAllowed(endpoint))
            throw new InvalidOperationException("MCP endpoint must be a public HTTPS URL.");

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
        var version = "2025-03-26";
        using var initialize = await SendAsync("initialize", new
        {
            protocolVersion = version, capabilities = new { },
            clientInfo = new { name = "AgentForge", version = "0.1" }
        }, 1, null);
        initialize.EnsureSuccessStatusCode();
        using (var response = await ReadResponseAsync(initialize, cancellationToken))
        {
            if (!response.RootElement.TryGetProperty("result", out var result) ||
                !result.TryGetProperty("protocolVersion", out var protocolVersion) ||
                protocolVersion.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(protocolVersion.GetString()))
                throw new InvalidDataException("Invalid MCP initialize response.");
            version = protocolVersion.GetString()!;
        }
        var session = initialize.Headers.TryGetValues("Mcp-Session-Id", out var values)
            ? values.FirstOrDefault() : null;

        using (var notification = new HttpRequestMessage(HttpMethod.Post, endpoint))
        {
            SetHeaders(notification, session);
            notification.Content = new StringContent(
                "{\"jsonrpc\":\"2.0\",\"method\":\"notifications/initialized\"}",
                Encoding.UTF8, "application/json");
            using var acknowledged = await client.SendAsync(notification, cancellationToken);
            acknowledged.EnsureSuccessStatusCode();
        }

        using var call = await SendAsync("tools/call", new { name = toolName, arguments }, 2, session);
        call.EnsureSuccessStatusCode();
        using var document = await ReadResponseAsync(call, cancellationToken);
        if (document.RootElement.TryGetProperty("error", out var error))
            throw new InvalidDataException("MCP server returned a JSON-RPC error: " + error.GetRawText());
        if (!document.RootElement.TryGetProperty("result", out var toolResult) ||
            toolResult.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Invalid MCP tools/call response.");
        return toolResult.Clone();

        async Task<HttpResponseMessage> SendAsync(string method, object parameters, int id, string? sessionId)
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
            SetHeaders(message, sessionId);
            message.Content = new StringContent(
                JsonSerializer.Serialize(new { jsonrpc = "2.0", id, method, @params = parameters }),
                Encoding.UTF8, "application/json");
            return await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }

        void SetHeaders(HttpRequestMessage message, string? sessionId)
        {
            message.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
            message.Headers.TryAddWithoutValidation("MCP-Protocol-Version", version);
            if (sessionId is not null)
                message.Headers.TryAddWithoutValidation("Mcp-Session-Id", sessionId);
        }
    }

    private static async Task<JsonDocument> ReadResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int count;
        while ((count = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + count > 1_048_576)
                throw new InvalidDataException("MCP response exceeds 1 MiB.");
            buffer.Write(chunk, 0, count);
        }
        var content = Encoding.UTF8.GetString(buffer.ToArray());
        if (response.Content.Headers.ContentType?.MediaType == "text/event-stream")
        {
            content = content.Split('\n')
                .Where(line => line.StartsWith("data:", StringComparison.Ordinal))
                .Select(line => line[5..].Trim())
                .FirstOrDefault(line => line.StartsWith('{'))
                ?? throw new InvalidDataException("Missing MCP JSON-RPC SSE event.");
        }
        return JsonDocument.Parse(content);
    }
}
