using System.Net.Http.Json;
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
        
        using var response = await client.PostAsJsonAsync(
            "chat/completions", 
            request,
            cancellationToken);

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