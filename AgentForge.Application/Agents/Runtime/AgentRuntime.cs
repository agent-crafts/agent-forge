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
    private const int MaxInferences = 5;
    private const int MaxToolCalls = 8;
    private const int MaxToolResultChars = 32768;

    public async Task<AgentRunResult> RunAsync(AgentRunRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var agent = await agents.GetByIdAsync(request.AgentId, cancellationToken);
        if (agent is null || !agent.IsActive)
            return new AgentRunResult(Guid.Empty, AgentRunOutcome.AgentNotFound);

        var context = CreateContext(agent, request.Input);
        var timer = Stopwatch.StartNew();
        context.Record("run.started", "success", new { runId = context.RunId, agentId = request.AgentId });

        try
        {
            var available = await LoadAvailableToolsAsync(request.AgentId, cancellationToken);
            var definitions = BuildToolDefinitions(available);
            return await ExecuteLoopAsync(context, available, definitions, timer, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Agent run {RunId} failed for agent {AgentId}",
                context.RunId, request.AgentId);
            return CompleteFailure(context, timer, "LLM request failed. Check provider configuration and server logs.");
        }
    }

    private static AgentRunContext CreateContext(Agent agent, string input)
    {
        var context = new AgentRunContext(agent.Id);
        if (!string.IsNullOrWhiteSpace(agent.SystemInstructions))
            context.AddMessage(new SystemMessage(agent.SystemInstructions));
        context.AddMessage(new UserMessage(input));
        return context;
    }

    private async Task<IReadOnlyDictionary<string, Tool>> LoadAvailableToolsAsync(
        Guid agentId, CancellationToken cancellationToken)
    {
        var allowedIds = (await agentTools.GetToolIdsAsync(agentId, cancellationToken)).ToHashSet();
        if (allowedIds.Count == 0)
            return new Dictionary<string, Tool>(StringComparer.Ordinal);

        // Load the catalog once, then enforce the agent's persisted allowlist in memory.
        var catalog = await tools.GetAllAsync(cancellationToken);
        var available = new Dictionary<string, Tool>(StringComparer.Ordinal);
        foreach (var tool in catalog)
        {
            if (!allowedIds.Contains(tool.Id) || !tool.IsActive)
                continue;
            if (!available.TryAdd(tool.Name, tool))
                throw new InvalidOperationException("Duplicate tool names are assigned to this agent.");
        }
        return available;
    }

    private static IReadOnlyList<LlmToolDefinition> BuildToolDefinitions(
        IReadOnlyDictionary<string, Tool> available)
    {
        var definitions = new List<LlmToolDefinition>(available.Count);
        foreach (var tool in available.Values)
        {
            using var schema = JsonDocument.Parse(tool.JsonSchema);
            definitions.Add(new LlmToolDefinition(tool.Name, tool.Description, schema.RootElement.Clone()));
        }
        return definitions;
    }

    private async Task<AgentRunResult> ExecuteLoopAsync(
        AgentRunContext context,
        IReadOnlyDictionary<string, Tool> available,
        IReadOnlyList<LlmToolDefinition> definitions,
        Stopwatch timer,
        CancellationToken cancellationToken)
    {
        var executedCalls = 0;
        for (var step = 0; step < MaxInferences; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await InferAsync(context, definitions, cancellationToken);

            if (response.ToolCalls.Count == 0)
                return string.IsNullOrWhiteSpace(response.Content)
                    ? CompleteFailure(context, timer, "LLM did not return a text response.")
                    : CompleteSuccess(context, timer, response.Content);

            if (step == MaxInferences - 1 || response.ToolCalls.Count > MaxToolCalls - executedCalls)
                return CompleteFailure(context, timer, "Agent execution limit reached.");

            if (!AreToolCallsAllowed(response.ToolCalls, available))
                return CompleteFailure(context, timer, "LLM requested an unavailable tool or invalid arguments.");

            context.AddMessage(new AssistantMessage(response.Content, response.ToolCalls));
            await ExecuteToolCallsAsync(context, response.ToolCalls, available, cancellationToken);
            executedCalls += response.ToolCalls.Count;
        }

        return CompleteFailure(context, timer, "Agent execution limit reached.");
    }

    private async Task<LlmChatResponse> InferAsync(
        AgentRunContext context,
        IReadOnlyList<LlmToolDefinition> definitions,
        CancellationToken cancellationToken)
    {
        var stepName = $"Inference #{context.NextInference()}";
        RecordInferenceRequest(context, stepName, definitions.Count > 0);

        var timer = Stopwatch.StartNew();
        try
        {
            var response = await llmClient.ChatAsync(
                new LlmChatRequest(context.Messages.ToArray(), definitions.Count > 0 ? definitions : null),
                cancellationToken);
            timer.Stop();
            RecordInferenceResponse(context, stepName, response, timer.ElapsedMilliseconds);
            return response;
        }
        catch
        {
            timer.Stop();
            context.Record("llm.response", "failed", new { message = "LLM request failed" },
                timer.ElapsedMilliseconds);
            throw;
        }
    }

    private static void RecordInferenceRequest(AgentRunContext context, string stepName, bool toolsEnabled) =>
        context.Record("llm.request", "success", new
        {
            stepName,
            arguments = new
            {
                messageCount = context.Messages.Count,
                messageRoles = context.Messages.Select(message => message switch
                {
                    SystemMessage => "system",
                    UserMessage => "user",
                    AssistantMessage => "assistant",
                    ToolResultMessage => "tool",
                    _ => "other"
                }).ToArray(),
                toolsEnabled
            }
        });

    private static void RecordInferenceResponse(
        AgentRunContext context, string stepName, LlmChatResponse response, long durationMs) =>
        context.Record("llm.response",
            response.ToolCalls.Count > 0 || !string.IsNullOrWhiteSpace(response.Content) ? "success" : "failed",
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
            }, durationMs);

    private static bool AreToolCallsAllowed(
        IReadOnlyList<LlmToolCall> calls, IReadOnlyDictionary<string, Tool> available) =>
        calls.All(call =>
            !string.IsNullOrWhiteSpace(call.Id) &&
            !string.IsNullOrWhiteSpace(call.Name) &&
            available.ContainsKey(call.Name) &&
            call.Arguments.ValueKind == JsonValueKind.Object);

    private async Task ExecuteToolCallsAsync(
        AgentRunContext context,
        IReadOnlyList<LlmToolCall> calls,
        IReadOnlyDictionary<string, Tool> available,
        CancellationToken cancellationToken)
    {
        foreach (var call in calls)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tool = available[call.Name];
            var timer = Stopwatch.StartNew();
            context.Record("tool.request", "success", new { toolName = call.Name, toolCallId = call.Id });

            var execution = await executionGateway.ExecuteAsync(tool, call.Arguments, cancellationToken);
            timer.Stop();
            context.Record("tool.response", execution.Success ? "success" : "failed",
                new { toolName = call.Name, toolCallId = call.Id, errorCode = execution.Error?.Code },
                timer.ElapsedMilliseconds);
            context.AddMessage(new ToolResultMessage(call.Id, SerializeToolResult(execution)));
        }
    }

    private static string SerializeToolResult(ToolExecutionResult execution)
    {
        var result = execution.Success
            ? JsonSerializer.Serialize(new { success = true, data = execution.Data })
            : JsonSerializer.Serialize(new { success = false, error = execution.Error });

        return result.Length <= MaxToolResultChars
            ? result
            : JsonSerializer.Serialize(new
            {
                success = false,
                error = new { code = "tool_result_too_large", message = "Tool output exceeds the run limit." }
            });
    }

    private static AgentRunResult CompleteSuccess(
        AgentRunContext context, Stopwatch timer, string response)
    {
        timer.Stop();
        context.Record("run.completed", "success",
            new { startedAt = context.StartedAt }, timer.ElapsedMilliseconds);
        return new AgentRunResult(context.RunId, AgentRunOutcome.Completed,
            Response: response, Trace: context.Trace);
    }

    private static AgentRunResult CompleteFailure(
        AgentRunContext context, Stopwatch timer, string error)
    {
        timer.Stop();
        context.Record("run.completed", "failed",
            new { startedAt = context.StartedAt }, timer.ElapsedMilliseconds);
        return new AgentRunResult(context.RunId, AgentRunOutcome.Failed,
            Error: error, Trace: context.Trace);
    }
}
