namespace AgentForge.Core.Repositories;

/// <summary>One page of records and the total number matching the query.</summary>
public sealed record PagedResult<TEntity>(
    IReadOnlyList<TEntity> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
