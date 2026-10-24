using AgentForge.Core.Enums;

namespace AgentForge.Core.Application.Tools.Commands;

public sealed record CreateToolCommand(
    string Name,
    string DisplayName,
    string Description,
    ToolKind Kind,
    ToolCategory Category,
    string JsonSchema,
    string? ImageUrl = null);
