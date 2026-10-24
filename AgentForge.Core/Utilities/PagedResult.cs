namespace AgentForge.Core.Utilities;

public sealed record PagedResult<TEntity>(IReadOnlyList<TEntity> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
