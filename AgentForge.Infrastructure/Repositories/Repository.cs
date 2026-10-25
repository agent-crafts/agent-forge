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
        PagedQuery<TEntity> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.PageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(request), "Page number must be positive.");
        if (request.PageSize is < 1 or > 200)
            throw new ArgumentOutOfRangeException(nameof(request), "Page size must be between 1 and 200.");

        IQueryable<TEntity> query = Entities.AsNoTracking();
        if (request.Filter is not null)
            query = query.Where(request.Filter);

        var totalCount = await query.CountAsync(cancellationToken);
        query = request.OrderBy is null
            ? query.OrderBy(entity => entity.Id)
            : request.OrderBy(query).ThenBy(entity => entity.Id);

        var items = await query
            .Skip(checked((request.PageNumber - 1) * request.PageSize))
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TEntity>(items, request.PageNumber, request.PageSize, totalCount);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await Entities.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => Entities.Update(entity);

    public void Remove(TEntity entity) => Entities.Remove(entity);
}
