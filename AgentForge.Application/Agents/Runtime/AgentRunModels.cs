namespace AgentForge.Application.Agents.Runtime;

public sealed record AgentRunRequest(Guid AgentId, string Input);

public enum AgentRunOutcome { Completed, AgentNotFound, Failed }

public sealed record AgentRunTraceEvent(
    string Type, string Status, DateTimeOffset Timestamp, long DurationMs, object Details);

public sealed record AgentRunResult(
    Guid RunId, AgentRunOutcome Outcome, string? Response = null, string? Error = null,
    IReadOnlyList<AgentRunTraceEvent>? Trace = null);
