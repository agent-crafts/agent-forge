using System.Net.Http.Headers;
using System.Text.Json;
using AgentForge.Core.Clients.Http;

namespace AgentForge.Infrastructure.Clients.Http;

public sealed class HttpApiClient(HttpClient client) : IHttpApiClient
{
    private const int MaxResponseBytes = 1_048_576;

    public async Task<JsonElement> GetJsonAsync(Uri url, CancellationToken cancellationToken = default)
    {
        if (url.Scheme != Uri.UriSchemeHttps || url.UserInfo.Length != 0 || url.Port != 443 ||
            !string.Equals(url.Host, "fakestoreapi.com", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only HTTPS URLs on fakestoreapi.com are approved.", nameof(url));

        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentType?.MediaType is not ("application/json" or "application/problem+json"))
            throw new InvalidDataException("API did not return JSON.");
        if (response.Content.Headers.ContentLength is > MaxResponseBytes)
            throw new InvalidDataException("Response exceeds 1 MiB.");

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int count;
        while ((count = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + count > MaxResponseBytes)
                throw new InvalidDataException("Response exceeds 1 MiB.");
            buffer.Write(chunk, 0, count);
        }
        buffer.Position = 0;
        using var document = await JsonDocument.ParseAsync(buffer, cancellationToken: cancellationToken);
        return document.RootElement.Clone();
    }
}
