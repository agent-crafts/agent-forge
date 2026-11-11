using System.Text.Json;

namespace AgentForge.Core.Clients;

// Message variants model valid conversation roles without unrelated nullable fields.
public abstract record LlmMessage;

public sealed record SystemMessage(string Content) : LlmMessage;

public sealed record UserMessage(string Content) : LlmMessage;

public sealed record AssistantMessage(
    string? Content,
    IReadOnlyList<LlmToolCall> ToolCalls) : LlmMessage;

public sealed record ToolResultMessage(
    string ToolCallId,
    string Content) : LlmMessage;

public sealed record LlmToolDefinition(string Name, string Description, JsonElement Parameters);

public sealed record LlmToolCall(string Id, string Name, JsonElement Arguments);

public sealed record LlmChatRequest(
    IReadOnlyList<LlmMessage> Messages,
    IReadOnlyList<LlmToolDefinition>? Tools = null);

public sealed record LlmChatResponse(
    string? Content,
    IReadOnlyList<LlmToolCall> ToolCalls,
    string? FinishReason);
