using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Agents.Commands;

public sealed record SetAgentToolsCommand(Guid AgentId, Guid[] ToolIds) : ICommand<bool>;

public sealed class SetAgentToolsCommandHandler(
    IRepository<Agent> agents, IToolRepository tools, IAgentToolRepository assignments, IUnitOfWork unitOfWork)
    : ICommandHandler<SetAgentToolsCommand, bool>
{
    public async ValueTask<bool> Handle(SetAgentToolsCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ToolIds is null || command.ToolIds.Any(id => id == Guid.Empty) ||
            command.ToolIds.Distinct().Count() != command.ToolIds.Length)
            throw new ArgumentException("Tool IDs must be unique, valid GUIDs.");

        var agent = await agents.GetByIdAsync(command.AgentId, cancellationToken);
        if (agent is null || !agent.IsActive) return false;

        foreach (var id in command.ToolIds)
        {
            var tool = await tools.GetByIdAsync(id, cancellationToken);
            if (tool is null || !tool.IsActive)
                throw new ArgumentException("All selected tools must exist and be active.");
        }

        await assignments.ReplaceAsync(command.AgentId, command.ToolIds, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
