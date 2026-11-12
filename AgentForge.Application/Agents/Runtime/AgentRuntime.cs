using System.Diagnostics;
using AgentForge.Core.Clients;
using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace AgentForge.Application.Agents.Runtime;

public sealed class AgentRuntime(
    IRepository<Agent> agents,
    ILlmClient llmClient,
    ILogger<AgentRuntime> logger) : IAgentRuntime
{
    public async Task<AgentRunResult> RunAsync(AgentRunRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var agent = await agents.GetByIdAsync(request.AgentId, cancellationToken);
        if (agent is null || !agent.IsActive)
            return new AgentRunResult(Guid.Empty, AgentRunOutcome.AgentNotFound);

        var context = new AgentRunContext(request.AgentId);
        if (!string.IsNullOrWhiteSpace(agent.SystemInstructions))
            context.AddMessage(new SystemMessage(agent.SystemInstructions));
        context.AddMessage(new UserMessage(request.Input));

        var timer = Stopwatch.StartNew();
        context.Record("run.started", "success", new { runId = context.RunId, agentId = request.AgentId });

        var inference = context.NextInference();
        var stepName = $"Inference #{inference}";
        context.Record("llm.request", "success", new
        {
            stepName,
            arguments = new
            {
                messageCount = context.Messages.Count,
                messageRoles = context.Messages.Select(m => m switch
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
                // PR 1: one inference only. Tool execution belongs to the next milestone.
                response = await llmClient.ChatAsync(new LlmChatRequest(context.Messages), cancellationToken);
            }
            catch
            {
                llmTimer.Stop();
                context.Record("llm.response", "failed", new { message = "LLM request failed" },
                    llmTimer.ElapsedMilliseconds);
                throw;
            }

            llmTimer.Stop();
            var valid = response.ToolCalls.Count == 0 && !string.IsNullOrWhiteSpace(response.Content);
            context.Record("llm.response", valid ? "success" : "failed",
                new
                {
                    stepName,
                    model = response.Model,
                    providerRequestId = response.ProviderRequestId,
                    finishReason = response.FinishReason,
                    toolCallCount = response.ToolCalls.Count,
                    usage = response.Usage,
                    costUsd = (decimal?)null,
                    costStatus = "Pricing not configured"
                }, llmTimer.ElapsedMilliseconds);

            timer.Stop();
            context.Record("run.completed", valid ? "success" : "failed",
                new { startedAt = context.StartedAt }, timer.ElapsedMilliseconds);

            return valid
                ? new AgentRunResult(context.RunId, AgentRunOutcome.Completed,
                    Response: response.Content, Trace: context.Trace)
                : new AgentRunResult(context.RunId, AgentRunOutcome.Failed,
                    Error: "LLM did not return a text response.", Trace: context.Trace);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            timer.Stop();
            context.Record("run.completed", "failed",
                new { startedAt = context.StartedAt }, timer.ElapsedMilliseconds);
            logger.LogError(exception, "Agent run {RunId} failed for agent {AgentId}",
                context.RunId, request.AgentId);
            return new AgentRunResult(context.RunId, AgentRunOutcome.Failed,
                Error: "LLM request failed. Check provider configuration and server logs.",
                Trace: context.Trace);
        }
    }
}
