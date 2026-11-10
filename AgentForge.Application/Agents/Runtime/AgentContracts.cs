namespace AgentForge.Application.Agents.Runtime;

public sealed record AgentMessage(string Role, string Content);

public abstract record AgentDecision;

public sealed record AgentFinish(string Answer) : AgentDecision;

public sealed record AgentToolCall(string ToolName, string ArgumentsJson) : AgentDecision;

public interface IAgentDecisionModel
{
    Task<AgentDecision> DecideAsync(
        IReadOnlyList<AgentMessage> messages,
        CancellationToken cancellationToken);
}

public interface IAgentToolInvoker
{
    Task<string> InvokeAsync(
        string toolName,
        string argumentsJson,
        CancellationToken cancellationToken);
}

public sealed record AgentRunResult(
    string Answer,
    int Steps,
    IReadOnlyList<AgentMessage> Messages);
