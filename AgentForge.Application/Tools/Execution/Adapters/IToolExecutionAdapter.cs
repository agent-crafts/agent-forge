using System.Text.Json;
using AgentForge.Core.Domains;

namespace AgentForge.Application.Tools.Execution;

/// <summary>An adapter handles a class of tools, not a hard-coded tool name.</summary>
public interface IToolExecutionAdapter
{
    bool CanHandle(Tool tool);
    Task<ToolExecutionResult> ExecuteAsync(Tool tool, JsonElement arguments, CancellationToken cancellationToken);
}
