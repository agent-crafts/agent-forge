using System.Net;
using System.Net.ServerSentEvents;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using AgentForge.Core.Clients.Http;

namespace AgentForge.Application.Tools.Execution.Mcp;

/// <summary>Executes a single Streamable HTTP MCP tool call against a public HTTPS endpoint.</summary>
public sealed class McpToolClient
{
    private const int MaxResponseBytes = 1_048_576;

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
        using var client = new HttpClient(handler);
        return await ExecuteProtocolAsync(client, endpoint, toolName, arguments, cancellationToken);
    }

    private static async Task<JsonElement> ExecuteProtocolAsync(
        HttpClient client, Uri endpoint, string toolName, JsonElement arguments, CancellationToken callerToken)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(callerToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(30));
        var cancellationToken = deadline.Token;
        var version = "2025-03-26";
        using var initialize = await SendAsync("initialize", new
        {
            protocolVersion = version, capabilities = new { },
            clientInfo = new { name = "AgentForge", version = "0.1" }
        }, 1, null);
        initialize.EnsureSuccessStatusCode();
        var response = await ReadResponseAsync(initialize, 1, cancellationToken);
        if (!response.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.Object ||
            !result.TryGetProperty("protocolVersion", out var protocolVersion) ||
            protocolVersion.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(protocolVersion.GetString()))
            throw new InvalidDataException("Invalid MCP initialize response.");
        version = protocolVersion.GetString()!;
        var session = initialize.Headers.TryGetValues("Mcp-Session-Id", out var values)
            ? values.FirstOrDefault() : null;

        using (var notification = new HttpRequestMessage(HttpMethod.Post, endpoint))
        {
            SetHeaders(notification, session);
            notification.Content = new StringContent(
                "{\"jsonrpc\":\"2.0\",\"method\":\"notifications/initialized\"}",
                Encoding.UTF8, "application/json");
            using var acknowledged = await client.SendAsync(notification, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            acknowledged.EnsureSuccessStatusCode();
        }

        using var call = await SendAsync("tools/call", new { name = toolName, arguments }, 2, session);
        call.EnsureSuccessStatusCode();
        var callResponse = await ReadResponseAsync(call, 2, cancellationToken);
        if (callResponse.TryGetProperty("error", out _))
            throw new InvalidDataException("MCP server returned a JSON-RPC error.");
        if (!callResponse.TryGetProperty("result", out var toolResult) ||
            toolResult.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Invalid MCP tools/call response.");
        return toolResult;

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

    private static async Task<JsonElement> ReadResponseAsync(
        HttpResponseMessage response, int requestId, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var boundedStream = new ResponseStream(stream);
        if (response.Content.Headers.ContentType?.MediaType == "text/event-stream")
        {
            await foreach (var item in SseParser.Create(boundedStream).EnumerateAsync(cancellationToken))
            {
                if (string.IsNullOrWhiteSpace(item.Data)) continue;
                using var document = JsonDocument.Parse(item.Data);
                if (FindResponse(document.RootElement, requestId) is { } match)
                    return match.Clone();
            }
        }
        else
        {
            using var document = await JsonDocument.ParseAsync(boundedStream, cancellationToken: cancellationToken);
            if (FindResponse(document.RootElement, requestId) is { } match)
                return match.Clone();
        }

        throw new InvalidDataException("MCP response did not match the JSON-RPC request ID.");
    }

    private static JsonElement? FindResponse(JsonElement message, int requestId)
    {
        if (message.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in message.EnumerateArray())
                if (IsResponse(entry)) return entry;
            return null;
        }

        return IsResponse(message) ? message : null;

        bool IsResponse(JsonElement entry) => entry.ValueKind == JsonValueKind.Object &&
            entry.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.Number &&
            id.TryGetInt32(out var value) && value == requestId &&
            (entry.TryGetProperty("result", out _) || entry.TryGetProperty("error", out _));
    }

    private sealed class ResponseStream(Stream source) : Stream
    {
        private int _remaining = MaxResponseBytes;

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var read = await source.ReadAsync(buffer[..Math.Min(buffer.Length, _remaining + 1)], cancellationToken);
            _remaining -= read;
            if (_remaining < 0) throw new InvalidDataException("MCP response exceeds 1 MiB.");
            return read;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
