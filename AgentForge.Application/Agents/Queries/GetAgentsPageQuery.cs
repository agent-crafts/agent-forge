using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using AgentForge.Core.Utilities;
using Mediator;

namespace AgentForge.Application.Agents.Queries;

public sealed record GetAgentsPageQuery(PagedQuery<Agent> Page) : IQuery<PagedResult<Agent>>;

public sealed class GetAgentsPageQueryHandler(IRepository<Agent> agents)
    : IQueryHandler<GetAgentsPageQuery, PagedResult<Agent>>
{
    public async ValueTask<PagedResult<Agent>> Handle(GetAgentsPageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.Page);
        return await agents.GetPagedAsync(query.Page, cancellationToken);
    }
}
