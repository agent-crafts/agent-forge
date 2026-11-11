using System.Diagnostics;
using AgentForge.Core.Clients;
using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AgentForge.Application.Agents.Commands;

public sealed record PlaygroundChatCommand(Guid AgentId, string? Message) : ICommand<PlaygroundChatResult>;

public sealed record PlaygroundTraceEvent(
    string Type, string Status, DateTimeOffset Timestamp, long DurationMs, object Details);

public enum PlaygroundChatOutcome { Success, InvalidMessage, AgentNotFound, ProviderFailure }

public sealed record PlaygroundChatResult(
    PlaygroundChatOutcome Outcome, string? Response = null, string? Error = null,
    IReadOnlyList<PlaygroundTraceEvent>? Trace = null);

public sealed class PlaygroundChatCommandHandler(
    IRepository<Agent> agents, ILlmClient llmClient, ILogger<PlaygroundChatCommandHandler> logger)
    : ICommandHandler<PlaygroundChatCommand, PlaygroundChatResult>
{
    public async ValueTask<PlaygroundChatResult> Handle(
        PlaygroundChatCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var message = command.Message?.Trim();
        if (string.IsNullOrWhiteSpace(message) || message.Length > 8000)
            return new(PlaygroundChatOutcome.InvalidMessage, Error: "Message must contain 1–8000 characters.");

        var agent = await agents.GetByIdAsync(command.AgentId, cancellationToken);
        if (agent is null || !agent.IsActive)
            return new(PlaygroundChatOutcome.AgentNotFound);

        var messages = new List<LlmMessage>();
        if (!string.IsNullOrWhiteSpace(agent.SystemInstructions))
            messages.Add(new SystemMessage(agent.SystemInstructions));
        messages.Add(new UserMessage(message));

        var runId = Guid.NewGuid();
        var startedAt = DateTimeOffset.UtcNow;
        var timer = Stopwatch.StartNew();
        var trace = new List<PlaygroundTraceEvent>();
        void Record(string type, string status, object details, long durationMs = 0) =>
            trace.Add(new PlaygroundTraceEvent(type, status, DateTimeOffset.UtcNow, durationMs, details));

        Record("run.started", "success", new { runId, agentId = command.AgentId });
        Record("llm.request", "success", new
        {
            stepName = "Inference #1",
            arguments = new
            {
                messageCount = messages.Count,
                messageRoles = messages.Select(m => m switch
                {
                    SystemMessage => "system", UserMessage => "user", _ => "other"
                }).ToArray(),
                toolsEnabled = false
            }
        });

        try
        {
            var llmTimer = Stopwatch.StartNew();
            LlmChatResponse response;
            try
            {
                // Playground is intentionally text-only; agent orchestration is separate.
                response = await llmClient.ChatAsync(new LlmChatRequest(messages), cancellationToken);
            }
            catch
            {
                llmTimer.Stop();
                Record("llm.response", "failed", new { message = "LLM request failed" },
                    llmTimer.ElapsedMilliseconds);
                throw;
            }

            llmTimer.Stop();
            var valid = response.ToolCalls.Count == 0 && !string.IsNullOrWhiteSpace(response.Content);
            Record("llm.response", valid ? "success" : "failed",
                new
                {
                    stepName = "Inference #1",
                    model = response.Model,
                    providerRequestId = response.ProviderRequestId,
                    finishReason = response.FinishReason,
                    toolCallCount = response.ToolCalls.Count,
                    usage = response.Usage,
                    costUsd = (decimal?)null,
                    costStatus = "Pricing not configured"
                }, llmTimer.ElapsedMilliseconds);

            timer.Stop();
            Record("run.completed", valid ? "success" : "failed",
                new { startedAt }, timer.ElapsedMilliseconds);
            return valid
                ? new(PlaygroundChatOutcome.Success, Response: response.Content, Trace: trace)
                : new(PlaygroundChatOutcome.ProviderFailure,
                    Error: "LLM did not return a text response.", Trace: trace);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            timer.Stop();
            Record("run.completed", "failed", new { startedAt }, timer.ElapsedMilliseconds);
            logger.LogError(exception, "Playground LLM call failed for agent {AgentId}", command.AgentId);
            return new(PlaygroundChatOutcome.ProviderFailure,
                Error: "LLM request failed. Check provider configuration and server logs.", Trace: trace);
        }
    }
}
