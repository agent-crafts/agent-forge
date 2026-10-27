using AgentForge.Core.Domains;
using AgentForge.Core.Enums;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Agents.Commands;

public sealed record UpdateAgentCommand(
    Guid Id, string Name, string Description, AgentCategory Category,
    string SystemInstructions, string? Icon = null, string[]? Tags = null) : ICommand<bool>;

public sealed class UpdateAgentCommandHandler(IRepository<Agent> agents, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAgentCommand, bool>
{
    public async ValueTask<bool> Handle(UpdateAgentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var agent = await agents.GetByIdAsync(command.Id, cancellationToken);
        if (agent is null || !agent.IsActive) return false;

        agent.UpdateDetails(command.Name, command.Description, command.Category,
            command.SystemInstructions, command.Icon, command.Tags);
        agents.Update(agent);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
