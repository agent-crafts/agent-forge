namespace AgentForge.Core.Repositories;

public interface IAgentToolRepository
{
    Task<IReadOnlyList<Guid>> GetToolIdsAsync(Guid agentId, CancellationToken cancellationToken = default);
    Task ReplaceAsync(Guid agentId, IReadOnlyCollection<Guid> toolIds, CancellationToken cancellationToken = default);
    Task<bool> IsAllowedAsync(Guid agentId, Guid toolId, CancellationToken cancellationToken = default);
}
