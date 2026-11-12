namespace AgentForge.Application.Agents.Runtime;

public interface IAgentRuntime
{
    Task<AgentRunResult> RunAsync(AgentRunRequest request, CancellationToken cancellationToken);
}
