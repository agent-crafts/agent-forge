using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using AgentForge.Core.Utilities;
using Mediator;

namespace AgentForge.Application.Projects.Queries;

public sealed record GetProjectsPageQuery(PagedQuery<Project> Page) : IQuery<PagedResult<Project>>;

public sealed class GetProjectsPageQueryHandler(IRepository<Project> projects)
    : IQueryHandler<GetProjectsPageQuery, PagedResult<Project>>
{
    public async ValueTask<PagedResult<Project>> Handle(GetProjectsPageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await projects.GetPagedAsync(query.Page, cancellationToken);
    }
}
