namespace AgentForge.Application.Agents.Runtime;

/// <summary>
/// Model-independent bounded agent loop. Tool authorization and input validation
/// are responsibilities of the IAgentToolInvoker implementation.
/// </summary>
public sealed class AgentRuntime(
    IAgentDecisionModel model,
    IAgentToolInvoker tools)
{
    public async Task<AgentRunResult> RunAsync(
        string goal,
        int maxSteps = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(goal))
            throw new ArgumentException("An agent goal is required.", nameof(goal));
        if (maxSteps < 1)
            throw new ArgumentOutOfRangeException(nameof(maxSteps), "Step limit must be positive.");

        var messages = new List<AgentMessage> { new("user", goal) };

        for (var step = 1; step <= maxSteps; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var decision = await model.DecideAsync(messages.AsReadOnly(), cancellationToken)
                ?? throw new InvalidOperationException("Model returned no decision.");

            switch (decision)
            {
                case AgentFinish finish:
                    if (string.IsNullOrWhiteSpace(finish.Answer))
                        throw new InvalidOperationException("Model returned an empty final answer.");
                    messages.Add(new AgentMessage("assistant", finish.Answer));
                    return new AgentRunResult(finish.Answer, step, messages.AsReadOnly());

                case AgentToolCall call:
                    if (string.IsNullOrWhiteSpace(call.ToolName) ||
                        string.IsNullOrWhiteSpace(call.ArgumentsJson))
                        throw new InvalidOperationException("Model returned an invalid tool call.");

                    // Preserve the decision and the observation for the next model turn.
                    messages.Add(new AgentMessage(
                        "assistant", $"Tool call: {call.ToolName} {call.ArgumentsJson}"));
                    var observation = await tools.InvokeAsync(
                        call.ToolName, call.ArgumentsJson, cancellationToken);
                    messages.Add(new AgentMessage("tool", observation
                        ?? throw new InvalidOperationException("Tool returned no observation.")));
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported agent decision: {decision.GetType().Name}.");
            }
        }

        throw new InvalidOperationException(
            $"Agent exceeded its step limit ({maxSteps}).");
    }
}
