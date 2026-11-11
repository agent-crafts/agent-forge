using System.Net;
using System.Text;
using System.Text.Json;
using AgentForge.Core.Clients;
using AgentForge.Core.Services;
using AgentForge.Infrastructure.Clients;

await Run("text response and system/user serialization", async () =>
{
    using var handler = new StubHandler("""{"choices":[{"message":{"role":"assistant","content":"Hello"},"finish_reason":"stop"}]}""");
    var client = CreateClient(handler);
    var result = await client.ChatAsync(new LlmChatRequest([
        new SystemMessage("Be concise"),
        new UserMessage("Hi")
    ]), CancellationToken.None);
    Check(result.Content == "Hello" && result.ToolCalls.Count == 0 && result.FinishReason == "stop", "Text response mismatch");
    using var sent = JsonDocument.Parse(handler.LastRequest!);
    var messages = sent.RootElement.GetProperty("messages");
    Check(messages.GetArrayLength() == 2 && messages[0].GetProperty("role").GetString() == "system", "System role missing");
    Check(messages[1].GetProperty("content").GetString() == "Hi", "User message missing");
});

await Run("multiple tool calls and result correlation", async () =>
{
    using var handler = new StubHandler("""{"choices":[{"message":{"role":"assistant","content":null,"tool_calls":[{"id":"call_1","type":"function","function":{"name":"weather","arguments":"{\"city\":\"Cairo\"}"}},{"id":"call_2","type":"function","function":{"name":"weather","arguments":"{\"city\":\"Dubai\"}"}}]},"finish_reason":"tool_calls"}]}""");
    var client = CreateClient(handler);
    using var args = JsonDocument.Parse("""{"city":"Cairo"}""");
    using var schema = JsonDocument.Parse("""{"type":"object"}""");
    var calls = new[] { new LlmToolCall("call_1", "weather", args.RootElement.Clone()) };
    var result = await client.ChatAsync(new LlmChatRequest([
        new UserMessage("Weather?"),
        new AssistantMessage(null, calls),
        new ToolResultMessage("call_1", "28C")
    ], [new LlmToolDefinition("weather", "Weather lookup", schema.RootElement.Clone())]), CancellationToken.None);
    Check(result.ToolCalls.Count == 2, "Expected two tool calls");
    Check(result.ToolCalls[0].Arguments.GetProperty("city").GetString() == "Cairo", "First tool arguments incorrect");
    Check(result.ToolCalls[1].Id == "call_2", "Second tool call ID incorrect");
    using var sent = JsonDocument.Parse(handler.LastRequest!);
    var messages = sent.RootElement.GetProperty("messages");
    Check(messages[1].GetProperty("tool_calls")[0].GetProperty("id").GetString() == "call_1", "Assistant tool call missing");
    Check(messages[2].GetProperty("tool_call_id").GetString() == "call_1", "Tool result correlation missing");
    Check(sent.RootElement.GetProperty("tools")[0].GetProperty("function").GetProperty("name").GetString() == "weather", "Tool definition missing");
});

await Run("invalid tool arguments rejected", async () =>
{
    using var handler = new StubHandler("""{"choices":[{"message":{"role":"assistant","content":null,"tool_calls":[{"id":"call_1","type":"function","function":{"name":"weather","arguments":"not-json"}}]},"finish_reason":"tool_calls"}]}""");
    try
    {
        await CreateClient(handler).ChatAsync(new LlmChatRequest([new UserMessage("Weather?")]), CancellationToken.None);
        throw new Exception("Invalid arguments were accepted");
    }
    catch (JsonException) { }
});

await Run("missing choices rejected", async () =>
{
    using var handler = new StubHandler("""{"choices":[]}""");
    try
    {
        await CreateClient(handler).ChatAsync(new LlmChatRequest([new UserMessage("Hi")]), CancellationToken.None);
        throw new Exception("Missing choices were accepted");
    }
    catch (InvalidOperationException) { }
});

await Run("legacy GenerateAsync remains supported", async () =>
{
    using var handler = new StubHandler("""{"choices":[{"message":{"role":"assistant","content":"Legacy works"},"finish_reason":"stop"}]}""");
    Check(await CreateClient(handler).GenerateAsync("Hello", CancellationToken.None) == "Legacy works", "Legacy response mismatch");
});

Console.WriteLine("All LLM contract checks passed.");

static LlmClient CreateClient(StubHandler handler)
    => new(new HttpClient(handler) { BaseAddress = new Uri("https://example.com/") },
        new TestConfig());

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

static async Task Run(string name, Func<Task> test)
{
    await test();
    Console.WriteLine($"PASS: {name}");
}

sealed class TestConfig : IConfigService
{
    public AIConfig AI { get; } = new() { Provider = "OpenAI", ApiKey = "test-key", Model = "test-model" };
}

sealed class StubHandler(string response) : HttpMessageHandler
{
    public string? LastRequest { get; private set; }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = await request.Content!.ReadAsStringAsync(cancellationToken);
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(response, Encoding.UTF8, "application/json")
        };
    }
}
