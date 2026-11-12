using System.Diagnostics;
using System.Text.Json;
using AgentForge.Application.Tools.Execution;
using AgentForge.Core.Clients;
using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace AgentForge.Application.Agents.Runtime;

public sealed class AgentRuntime(
    IRepository<Agent> agents,
    IRepository<Tool> tools,
    IAgentToolRepository agentTools,
    IToolExecutionGateway executionGateway,
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

        try
        {
            // Persisted assignments are the authorization boundary, not the LLM's tool name.
            var allowedIds = await agentTools.GetToolIdsAsync(request.AgentId, cancellationToken);
            var available = new Dictionary<string, Tool>(StringComparer.Ordinal);
            var definitions = new List<LlmToolDefinition>();
            foreach (var id in allowedIds.Distinct())
            {
                var tool = await tools.GetByIdAsync(id, cancellationToken);
                if (tool is null || !tool.IsActive)
                    continue;
                if (!available.TryAdd(tool.Name, tool))
                    return Fail("Duplicate tool names are assigned to this agent.");

                using var schema = JsonDocument.Parse(tool.JsonSchema);
                definitions.Add(new LlmToolDefinition(tool.Name, tool.Description, schema.RootElement.Clone()));
            }

            const int maxInferences = 5;
            const int maxToolCalls = 8;
            var toolCallsExecuted = 0;

            for (var step = 0; step < maxInferences; step++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var stepName = $"Inference #{context.NextInference()}";
                context.Record("llm.request", "success", new
                {
                    stepName,
                    arguments = new
                    {
                        messageCount = context.Messages.Count,
                        messageRoles = context.Messages.Select(m => m switch
                        {
                            SystemMessage => "system", UserMessage => "user",
                            AssistantMessage => "assistant", ToolResultMessage => "tool", _ => "other"
                        }).ToArray(),
                        toolsEnabled = definitions.Count > 0
                    }
                });

                var llmTimer = Stopwatch.StartNew();
                LlmChatResponse response;
                try
                {
                    response = await llmClient.ChatAsync(
                        new LlmChatRequest(context.Messages.ToArray(), definitions.Count > 0 ? definitions : null),
                        cancellationToken);
                }
                catch
                {
                    llmTimer.Stop();
                    context.Record("llm.response", "failed", new { message = "LLM request failed" },
                        llmTimer.ElapsedMilliseconds);
                    throw;
                }

                llmTimer.Stop();
                var hasCalls = response.ToolCalls.Count > 0;
                var hasText = !string.IsNullOrWhiteSpace(response.Content);
                context.Record("llm.response", hasCalls || hasText ? "success" : "failed",
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

                if (!hasCalls)
                {
                    if (!hasText)
                        return Fail("LLM did not return a text response.");

                    timer.Stop();
                    context.Record("run.completed", "success",
                        new { startedAt = context.StartedAt }, timer.ElapsedMilliseconds);
                    return new AgentRunResult(context.RunId, AgentRunOutcome.Completed,
                        Response: response.Content, Trace: context.Trace);
                }

                if (step == maxInferences - 1 || response.ToolCalls.Count > maxToolCalls - toolCallsExecuted)
                    return Fail("Agent execution limit reached.");

                // Never execute a tool that was not resolved from this agent's active allowlist.
                if (response.ToolCalls.Any(call =>
                    string.IsNullOrWhiteSpace(call.Id) ||
                    !available.ContainsKey(call.Name) ||
                    call.Arguments.ValueKind != JsonValueKind.Object))
                    return Fail("LLM requested an unavailable tool or invalid arguments.");

                context.AddMessage(new AssistantMessage(response.Content, response.ToolCalls));
                foreach (var call in response.ToolCalls)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var tool = available[call.Name];
                    var toolTimer = Stopwatch.StartNew();
                    context.Record("tool.request", "success", new { toolName = call.Name, toolCallId = call.Id });
                    var execution = await executionGateway.ExecuteAsync(tool, call.Arguments, cancellationToken);
                    toolTimer.Stop();
                    toolCallsExecuted++;
                    context.Record("tool.response", execution.Success ? "success" : "failed",
                        new { toolName = call.Name, toolCallId = call.Id, errorCode = execution.Error?.Code },
                        toolTimer.ElapsedMilliseconds);

                    // A failed tool is still a tool result: the model can explain/recover.
                    // Do not expose exception internals or unbounded output to the model.
                    var result = execution.Success
                        ? JsonSerializer.Serialize(new { success = true, data = execution.Data })
                        : JsonSerializer.Serialize(new { success = false, error = execution.Error });
                    const int maxToolResultChars = 32768;
                    if (result.Length > maxToolResultChars)
                        result = JsonSerializer.Serialize(new
                        {
                            success = false,
                            error = new { code = "tool_result_too_large", message = "Tool output exceeds the run limit." }
                        });
                    context.AddMessage(new ToolResultMessage(call.Id, result));
                }
            }

            return Fail("Agent execution limit reached.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Agent run {RunId} failed for agent {AgentId}",
                context.RunId, request.AgentId);
            return Fail("LLM request failed. Check provider configuration and server logs.");
        }

        AgentRunResult Fail(string error)
        {
            timer.Stop();
            context.Record("run.completed", "failed",
                new { startedAt = context.StartedAt }, timer.ElapsedMilliseconds);
            return new AgentRunResult(context.RunId, AgentRunOutcome.Failed, Error: error, Trace: context.Trace);
        }
    }
}
