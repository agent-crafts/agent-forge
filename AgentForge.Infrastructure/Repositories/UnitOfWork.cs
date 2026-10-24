using AgentForge.Core.Repositories;
using AgentForge.Infrastructure.Persistence;

namespace AgentForge.Infrastructure.Repositories;

public sealed class UnitOfWork(AgentForgeDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
