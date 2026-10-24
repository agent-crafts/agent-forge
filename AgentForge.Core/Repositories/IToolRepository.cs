using AgentForge.Core.Domains;

namespace AgentForge.Core.Repositories;

/// <summary>Tool metadata persistence and tool-specific queries.</summary>
public interface IToolRepository : IRepository<Tool>
{
    Task<Tool?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}
