using AgentForge.Core.Domains;
using AgentForge.Core.Utilities;

namespace AgentForge.Core.Repositories;

public interface IRepository<TEntity> where TEntity : Base
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<TEntity>> GetPagedAsync(
        PagedQuery<TEntity> query,
        CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
