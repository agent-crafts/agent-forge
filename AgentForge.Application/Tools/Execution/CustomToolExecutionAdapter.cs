using System.Text.Json;
using AgentForge.Core.Domains;

namespace AgentForge.Application.Tools.Execution;

/// <summary>
/// Compatibility bridge for existing application-owned executors.
/// No new custom tool requires changes to the execution gateway.
/// </summary>
public sealed class CustomToolExecutionAdapter(IToolExecutorRegistry executors) : IToolExecutionAdapter
{
    public bool CanHandle(Tool tool) =>
        executors.CanExecute(tool.Name);

    public Task<ToolExecutionResult> ExecuteAsync(
        Tool tool, JsonElement arguments, CancellationToken cancellationToken)
    {
        var executor = executors.Resolve(tool.Name);
        return executor is null
            ? Task.FromResult(ToolExecutionResult.Fail("executor_unavailable", "No approved executor is registered."))
            : executor.ExecuteAsync(arguments, cancellationToken);
    }
}
