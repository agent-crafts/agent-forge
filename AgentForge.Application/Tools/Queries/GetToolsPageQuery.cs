using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using AgentForge.Core.Utilities;
using Mediator;

namespace AgentForge.Application.Tools.Queries;

public sealed record GetToolsPageQuery(PagedQuery<Tool> Page) : IQuery<PagedResult<Tool>>;

public sealed class GetToolsPageQueryHandler(IToolRepository tools) : IQueryHandler<GetToolsPageQuery, PagedResult<Tool>>
{
    public async ValueTask<PagedResult<Tool>> Handle(GetToolsPageQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await tools.GetPagedAsync(query.Page, cancellationToken);
    }
}
