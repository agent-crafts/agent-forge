using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using AgentForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgentForge.Infrastructure.Repositories;

/// <summary>Queries persisted tool metadata; does not register executable tools.</summary>
public sealed class ToolRepository(AgentForgeDbContext context)
    : Repository<Tool>(context), IToolRepository
{
    public Task<Tool?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return Entities.FirstOrDefaultAsync(tool => tool.Name == name, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return Entities.AnyAsync(tool => tool.Name == name, cancellationToken);
    }
}
