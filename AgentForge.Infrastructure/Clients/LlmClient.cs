using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using AgentForge.Core.Clients;
using AgentForge.Core.Services;

namespace AgentForge.Infrastructure.Clients;

public class LlmClient(HttpClient client, IConfigService config) : ILlmClient
{
    public async Task<string?> GenerateAsync(string prompt, CancellationToken cancellationToken)
    {
        var request = new
        {
            model = config.AI.Model,
            messages = new []
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            stream = false
        };
        
        if (string.Equals(config.AI.Provider, "OpenAI", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(config.AI.ApiKey))
            throw new InvalidOperationException("OpenAI API key is missing. Set AI__ApiKey or OPENAI_API_KEY in the environment.");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(request)
        };
        if (!string.IsNullOrWhiteSpace(config.AI.ApiKey))
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.AI.ApiKey);

        using var response = await client.SendAsync(httpRequest, cancellationToken);

        response.EnsureSuccessStatusCode();
        
        using var json = await response.Content.ReadFromJsonAsync<JsonDocument>(
            cancellationToken: cancellationToken);
        
        var content = json?
            .RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content
               ?? throw new InvalidOperationException(
                   "LLM returned an empty response.");
    }
}