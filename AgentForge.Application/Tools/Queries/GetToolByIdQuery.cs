using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Tools.Queries;

public sealed record GetToolByIdQuery(Guid Id) : IQuery<Tool?>;

public sealed class GetToolByIdQueryHandler(IToolRepository tools) : IQueryHandler<GetToolByIdQuery, Tool?>
{
    public async ValueTask<Tool?> Handle(GetToolByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await tools.GetByIdAsync(query.Id, cancellationToken);
    }
}
