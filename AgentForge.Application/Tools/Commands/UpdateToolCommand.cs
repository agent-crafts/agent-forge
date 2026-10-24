using AgentForge.Core.Enums;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Tools.Commands;

public sealed record UpdateToolCommand(
    Guid Id, string DisplayName, string Description,
    ToolCategory Category, string JsonSchema, string? ImageUrl = null) : ICommand<bool>;

public sealed class UpdateToolCommandHandler(IToolRepository tools, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateToolCommand, bool>
{
    public async ValueTask<bool> Handle(UpdateToolCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var tool = await tools.GetByIdAsync(command.Id, cancellationToken);
        if (tool is null || !tool.IsActive) return false;
        tool.UpdateDetails(command.DisplayName, command.Description, command.Category,
            command.JsonSchema, command.ImageUrl);
        tools.Update(tool);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
