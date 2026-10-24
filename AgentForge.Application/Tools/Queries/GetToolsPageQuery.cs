using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using AgentForge.Core.Utilities;

namespace AgentForge.Application.Tools.Queries;

public sealed record GetToolsPageQuery(PagedQuery<Tool> Page);

public sealed class GetToolsPageQueryHandler(IToolRepository tools)
{
    public Task<PagedResult<Tool>> HandleAsync(GetToolsPageQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return tools.GetPagedAsync(query.Page, cancellationToken);
    }
}
