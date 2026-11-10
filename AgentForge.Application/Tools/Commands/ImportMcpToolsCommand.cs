using AgentForge.Core.Domains;
using AgentForge.Core.Enums;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Tools.Commands;

public sealed record ImportMcpToolItem(string Name, string DisplayName, string Description, string JsonSchema, string ConfigurationJson);
public sealed record ImportMcpToolsCommand(IReadOnlyList<ImportMcpToolItem> Tools, ToolCategory Category = ToolCategory.Integration) : ICommand<int>;

public sealed class ImportMcpToolsCommandHandler(IToolRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<ImportMcpToolsCommand, int>
{
    public async ValueTask<int> Handle(ImportMcpToolsCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(command.Category))
            throw new ArgumentException("Invalid tool category.");
        if (command.Tools.Count is < 1 or > 200)
            throw new ArgumentException("Select between 1 and 200 MCP tools.");

        var tools = command.Tools.Select(item =>
            Tool.Create(item.Name, item.DisplayName, item.Description, ToolKind.External,
                command.Category, item.JsonSchema, executorKey: "mcp", configurationJson: item.ConfigurationJson)).ToArray();

        if (tools.Select(tool => tool.Name).Distinct(StringComparer.Ordinal).Count() != tools.Length)
            throw new ArgumentException("Selected tools must have unique names.");

        foreach (var tool in tools)
        {
            if (await repository.IsNameReservedAsync(tool.Name, cancellationToken))
                throw new InvalidOperationException("Tool already exists: " + tool.Name);
        }

        foreach (var tool in tools)
            await repository.AddAsync(tool, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return tools.Length;
    }
}
