using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Agents.Queries;

public sealed record GetAgentToolIdsQuery(Guid AgentId) : IQuery<IReadOnlyList<Guid>>;

public sealed class GetAgentToolIdsQueryHandler(IAgentToolRepository assignments)
    : IQueryHandler<GetAgentToolIdsQuery, IReadOnlyList<Guid>>
{
    public async ValueTask<IReadOnlyList<Guid>> Handle(GetAgentToolIdsQuery query, CancellationToken cancellationToken)
        => await assignments.GetToolIdsAsync(query.AgentId, cancellationToken);
}
