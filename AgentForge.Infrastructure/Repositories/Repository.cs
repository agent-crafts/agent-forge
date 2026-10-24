using System.Linq.Expressions;
using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using AgentForge.Core.Utilities;
using AgentForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgentForge.Infrastructure.Repositories;

public class Repository<TEntity>(AgentForgeDbContext context) : IRepository<TEntity>
    where TEntity : Base
{
    protected DbSet<TEntity> Entities => context.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Entities.FindAsync(new object[] { id }, cancellationToken);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Entities.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<PagedResult<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (pageSize is < 1 or > 200)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 200.");

        IQueryable<TEntity> query = Entities.AsNoTracking();
        if (filter is not null)
            query = query.Where(filter);

        var totalCount = await query.CountAsync(cancellationToken);
        query = orderBy is null
            ? query.OrderBy(entity => entity.Id)
            : orderBy(query).ThenBy(entity => entity.Id);

        var items = await query
            .Skip(checked((pageNumber - 1) * pageSize))
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TEntity>(items, pageNumber, pageSize, totalCount);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await Entities.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => Entities.Update(entity);

    public void Remove(TEntity entity) => Entities.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
