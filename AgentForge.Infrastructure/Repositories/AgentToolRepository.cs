using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using AgentForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgentForge.Infrastructure.Repositories;

public sealed class AgentToolRepository(AgentForgeDbContext db) : IAgentToolRepository
{
    public async Task<IReadOnlyList<Guid>> GetToolIdsAsync(Guid agentId, CancellationToken cancellationToken = default)
        => await db.Set<AgentTool>().AsNoTracking().Where(x => x.AgentId == agentId).Select(x => x.ToolId).ToListAsync(cancellationToken);

    public async Task<bool> IsAllowedAsync(Guid agentId, Guid toolId, CancellationToken cancellationToken = default)
        => await db.Set<AgentTool>().AnyAsync(x => x.AgentId == agentId && x.ToolId == toolId, cancellationToken);

    public async Task ReplaceAsync(Guid agentId, IReadOnlyCollection<Guid> toolIds, CancellationToken cancellationToken = default)
    {
        var current = await db.Set<AgentTool>().Where(x => x.AgentId == agentId).ToListAsync(cancellationToken);
        var desired = toolIds.ToHashSet();
        db.Set<AgentTool>().RemoveRange(current.Where(x => !desired.Contains(x.ToolId)));
        var existing = current.Select(x => x.ToolId).ToHashSet();
        await db.Set<AgentTool>().AddRangeAsync(desired.Except(existing).Select(id => new AgentTool(agentId, id)), cancellationToken);
    }
}
