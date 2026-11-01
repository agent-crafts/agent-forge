using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Agents.Queries;

public sealed record GetAgentByIdQuery(Guid Id) : IQuery<Agent?>;

public sealed class GetAgentByIdQueryHandler(IRepository<Agent> agents)
    : IQueryHandler<GetAgentByIdQuery, Agent?>
{
    public async ValueTask<Agent?> Handle(GetAgentByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await agents.GetByIdAsync(query.Id, cancellationToken);
    }
}
