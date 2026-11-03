using System.Text.Json;

namespace AgentForge.Core.Clients.Http;

public interface IHttpApiClient
{
    Task<JsonElement> GetJsonAsync(Uri url, CancellationToken cancellationToken = default);
}
