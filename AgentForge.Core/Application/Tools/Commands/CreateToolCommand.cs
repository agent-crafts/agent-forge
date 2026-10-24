using AgentForge.Core.Domains;
using AgentForge.Core.Enums;
using AgentForge.Core.Repositories;

namespace AgentForge.Core.Application.Tools.Commands;

public sealed record CreateToolCommand(
    string Name,
    string DisplayName,
    string Description,
    ToolKind Kind,
    ToolCategory Category,
    string JsonSchema,
    string? ImageUrl = null);

public sealed class CreateToolCommandHandler(IToolRepository tools, IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(CreateToolCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var tool = Tool.Create(command.Name, command.DisplayName, command.Description,
            command.Kind, command.Category, command.JsonSchema, command.ImageUrl);

        if (await tools.ExistsByNameAsync(tool.Name, cancellationToken))
            throw new InvalidOperationException("A tool with this name already exists.");

        await tools.AddAsync(tool, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return tool.Id;
    }
}
