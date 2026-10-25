using System.Linq.Expressions;

namespace AgentForge.Core.Utilities;

/// <summary>Options for a filtered and sorted page of entities.</summary>
public sealed record PagedQuery<TEntity>(
    int PageNumber = 1,
    int PageSize = 20,
    Expression<Func<TEntity, bool>>? Filter = null,
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy = null);
