using System.Text.Json;

namespace AgentForge.Application.Tools.Execution;

/// <summary>Only explicitly registered application capabilities may execute.</summary>
public interface IToolExecutor
{
    string ToolName { get; }
    Task<ToolExecutionResult> ExecuteAsync(JsonElement input, CancellationToken cancellationToken);
}
