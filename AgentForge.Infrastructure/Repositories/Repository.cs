using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using AgentForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgentForge.Infrastructure.Repositories;

/// <summary>EF Core implementation; changes are committed explicitly through SaveChangesAsync.</summary>
public class Repository<TEntity>(AgentForgeDbContext context) : IRepository<TEntity>
    where TEntity : Base
{
    protected DbSet<TEntity> Entities => context.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Entities.FindAsync(new object[] { id }, cancellationToken);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Entities.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await Entities.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => Entities.Update(entity);

    public void Remove(TEntity entity) => Entities.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
