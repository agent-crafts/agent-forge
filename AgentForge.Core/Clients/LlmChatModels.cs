using System.Text.Json;

namespace AgentForge.Core.Clients;

public enum LlmRole { System, User, Assistant, Tool }

public sealed record LlmMessage(
    LlmRole Role,
    string? Content,
    string? ToolCallId = null,
    IReadOnlyList<LlmToolCall>? ToolCalls = null);

public sealed record LlmToolDefinition(string Name, string Description, JsonElement Parameters);

public sealed record LlmToolCall(string Id, string Name, JsonElement Arguments);

public sealed record LlmChatRequest(
    IReadOnlyList<LlmMessage> Messages,
    IReadOnlyList<LlmToolDefinition>? Tools = null);

public sealed record LlmChatResponse(
    string? Content,
    IReadOnlyList<LlmToolCall> ToolCalls,
    string? FinishReason);
