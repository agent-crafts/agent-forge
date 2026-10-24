using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;

namespace AgentForge.Application.Tools.Queries;

public sealed record GetToolByIdQuery(Guid Id);

public sealed class GetToolByIdQueryHandler(IToolRepository tools)
{
    public Task<Tool?> HandleAsync(GetToolByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return tools.GetByIdAsync(query.Id, cancellationToken);
    }
}
