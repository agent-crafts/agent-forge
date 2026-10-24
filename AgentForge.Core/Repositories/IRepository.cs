using AgentForge.Core.Domains;

namespace AgentForge.Core.Repositories;

/// <summary>Persistence contract for entities with a shared identity.</summary>
public interface IRepository<TEntity> where TEntity : Base
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
