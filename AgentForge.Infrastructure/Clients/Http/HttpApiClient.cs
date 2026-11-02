using System.Text.Json;
using Microsoft.Extensions.Logging;
using AgentForge.Core.Clients.Http;

namespace AgentForge.Infrastructure.Clients.Http;

public sealed class HttpApiClient(HttpClient client, ILogger<HttpApiClient> logger) : IHttpApiClient
{
    private const int MaxResponseBytes = 1_048_576;

    public async Task<JsonElement> GetJsonAsync(Uri url, CancellationToken cancellationToken = default)
    {
        if (!PublicHttpUrlPolicy.IsAllowed(url))
            throw new ArgumentException("Only public HTTPS API URLs are allowed.", nameof(url));

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            // Log the exception chain server-side, but never log URL paths, queries or credentials.
            logger.LogError(exception, "HTTP JSON tool request failed for host {Host}", url.Host);
            throw;
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "HTTP JSON tool request timed out for host {Host}", url.Host);
            throw;
        }

        using (response)
        {
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
}
