using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AgentForge.Core.Clients;
using AgentForge.Core.Services;

namespace AgentForge.Infrastructure.Clients;

public sealed class LlmClient(HttpClient client, IConfigService config) : ILlmClient
{
    public async Task<string?> GenerateAsync(string prompt, CancellationToken token)
    {
        var response = await ChatAsync(new LlmChatRequest([new UserMessage(prompt)]), token);
        return response.Content ?? throw new InvalidOperationException("LLM returned an empty response.");
    }

    public async Task<LlmChatResponse> ChatAsync(LlmChatRequest request, CancellationToken token)
    {
        if (request.Messages is null || request.Messages.Count == 0)
            throw new ArgumentException("At least one chat message is required.", nameof(request));
        if (string.Equals(config.AI.Provider, "OpenAI", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(config.AI.ApiKey))
            throw new InvalidOperationException("OpenAI API key is missing. Set AI__ApiKey or OPENAI_API_KEY in the environment.");

        var messages = request.Messages.Select(message =>
        {
            var item = new Dictionary<string, object?>();
            switch (message)
            {
                case SystemMessage system:
                    item["role"] = "system";
                    item["content"] = system.Content;
                    break;
                case UserMessage user:
                    item["role"] = "user";
                    item["content"] = user.Content;
                    break;
                case AssistantMessage assistant:
                    item["role"] = "assistant";
                    item["content"] = assistant.Content;
                    if (assistant.ToolCalls.Count > 0)
                        item["tool_calls"] = assistant.ToolCalls.Select(call => new
                        {
                            id = call.Id,
                            type = "function",
                            function = new { name = call.Name, arguments = call.Arguments.GetRawText() }
                        }).ToArray();
                    break;
                case ToolResultMessage tool:
                    item["role"] = "tool";
                    item["tool_call_id"] = tool.ToolCallId;
                    item["content"] = tool.Content;
                    break;
                default:
                    throw new ArgumentException($"Unsupported LLM message type: {message?.GetType().Name ?? "null"}", nameof(request));
            }
            return item;
        }).ToArray();

        var payload = new Dictionary<string, object?>
        {
            ["model"] = config.AI.Model,
            ["messages"] = messages,
            ["stream"] = false
        };
        if (request.Tools is { Count: > 0 })
            payload["tools"] = request.Tools.Select(tool => new
            {
                type = "function",
                function = new { name = tool.Name, description = tool.Description, parameters = tool.Parameters }
            }).ToArray();

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(payload)
        };
        if (!string.IsNullOrWhiteSpace(config.AI.ApiKey))
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.AI.ApiKey);

        using var response = await client.SendAsync(httpRequest, token);
        response.EnsureSuccessStatusCode();
        using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: token)
            ?? throw new InvalidOperationException("LLM returned an empty JSON response.");
        var root = document.RootElement;
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
            throw new InvalidOperationException("LLM response does not contain choices.");
        var choice = choices[0];
        var message = choice.GetProperty("message");
        var content = message.TryGetProperty("content", out var contentValue) && contentValue.ValueKind == JsonValueKind.String
            ? contentValue.GetString() : null;
        var calls = new List<LlmToolCall>();
        if (message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.ValueKind == JsonValueKind.Array)
        {
            foreach (var call in toolCalls.EnumerateArray())
            {
                var function = call.GetProperty("function");
                var argsText = function.GetProperty("arguments").GetString()
                    ?? throw new InvalidOperationException("Tool call arguments are missing.");
                using var argsDocument = JsonDocument.Parse(argsText);
                if (argsDocument.RootElement.ValueKind != JsonValueKind.Object)
                    throw new InvalidOperationException("Tool call arguments must be a JSON object.");
                calls.Add(new LlmToolCall(call.GetProperty("id").GetString()!,
                    function.GetProperty("name").GetString()!, argsDocument.RootElement.Clone()));
            }
        }
        if (content is null && calls.Count == 0)
            throw new InvalidOperationException("LLM returned neither content nor tool calls.");
        var finishReason = choice.TryGetProperty("finish_reason", out var finish) && finish.ValueKind == JsonValueKind.String
            ? finish.GetString() : null;
        return new LlmChatResponse(content, calls, finishReason);
    }
}
