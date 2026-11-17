using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Projects.Queries;

public sealed record GetProjectByIdQuery(Guid Id) : IQuery<Project?>;

public sealed class GetProjectByIdQueryHandler(IRepository<Project> projects)
    : IQueryHandler<GetProjectByIdQuery, Project?>
{
    public async ValueTask<Project?> Handle(GetProjectByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await projects.GetByIdAsync(query.Id, cancellationToken);
    }
}
