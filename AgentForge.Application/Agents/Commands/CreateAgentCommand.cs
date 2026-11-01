using AgentForge.Core.Domains;
using AgentForge.Core.Enums;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Agents.Commands;

public sealed record CreateAgentCommand(
    string Name, string Description, AgentCategory Category,
    string SystemInstructions, string? Icon = null, string[]? Tags = null) : ICommand<Guid>;

public sealed class CreateAgentCommandHandler(IRepository<Agent> agents, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateAgentCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateAgentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var agent = Agent.Create(command.Name, command.Description, command.Category,
            command.SystemInstructions, command.Icon, command.Tags);
        await agents.AddAsync(agent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return agent.Id;
    }
}
