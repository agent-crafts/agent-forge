using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Tools.Commands;

/// <summary>Soft-deletes a tool by deactivating it; its persisted data remains intact.</summary>
public sealed record DeleteToolCommand(Guid Id) : ICommand<bool>;

public sealed class DeleteToolCommandHandler(IToolRepository tools, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteToolCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteToolCommand command, CancellationToken cancellationToken)
    {
        var tool = await tools.GetByIdAsync(command.Id, cancellationToken);
        if (tool is null || !tool.IsActive) return false;

        tool.Deactivate();
        tools.Update(tool);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
