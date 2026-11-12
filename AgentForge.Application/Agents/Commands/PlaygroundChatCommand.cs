using AgentForge.Application.Agents.Runtime;
using Mediator;

namespace AgentForge.Application.Agents.Commands;

public sealed record PlaygroundChatCommand(Guid AgentId, string? Message) : ICommand<PlaygroundChatResult>;

public sealed record PlaygroundTraceEvent(
    string Type, string Status, DateTimeOffset Timestamp, long DurationMs, object Details);

public enum PlaygroundChatOutcome { Success, InvalidMessage, AgentNotFound, ProviderFailure }

public sealed record PlaygroundChatResult(
    PlaygroundChatOutcome Outcome, string? Response = null, string? Error = null,
    IReadOnlyList<PlaygroundTraceEvent>? Trace = null);

public sealed class PlaygroundChatCommandHandler(IAgentRuntime runtime)
    : ICommandHandler<PlaygroundChatCommand, PlaygroundChatResult>
{
    public async ValueTask<PlaygroundChatResult> Handle(
        PlaygroundChatCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var message = command.Message?.Trim();
        if (string.IsNullOrWhiteSpace(message) || message.Length > 8000)
            return new(PlaygroundChatOutcome.InvalidMessage, Error: "Message must contain 1–8000 characters.");

        var result = await runtime.RunAsync(new AgentRunRequest(command.AgentId, message), cancellationToken);
        var trace = result.Trace?.Select(e =>
            new PlaygroundTraceEvent(e.Type, e.Status, e.Timestamp, e.DurationMs, e.Details)).ToArray();

        return result.Outcome switch
        {
            AgentRunOutcome.Completed => new(PlaygroundChatOutcome.Success,
                Response: result.Response, Trace: trace),
            AgentRunOutcome.AgentNotFound => new(PlaygroundChatOutcome.AgentNotFound),
            _ => new(PlaygroundChatOutcome.ProviderFailure, Error: result.Error, Trace: trace)
        };
    }
}
