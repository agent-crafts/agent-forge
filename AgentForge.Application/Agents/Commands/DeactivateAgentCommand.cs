using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Agents.Commands;

/// <summary>Soft-deactivates an agent definition; does not delete persisted data.</summary>
public sealed record DeactivateAgentCommand(Guid Id) : ICommand<bool>;

public sealed class DeactivateAgentCommandHandler(IRepository<Agent> agents, IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivateAgentCommand, bool>
{
    public async ValueTask<bool> Handle(DeactivateAgentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var agent = await agents.GetByIdAsync(command.Id, cancellationToken);
        if (agent is null || !agent.IsActive) return false;

        agent.Deactivate();
        agents.Update(agent);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
