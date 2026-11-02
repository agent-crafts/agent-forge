using System.Text.Json;
using AgentForge.Core.Domains;

namespace AgentForge.Application.Tools.Execution;

public sealed class ToolExecutionGateway(IEnumerable<IToolExecutionAdapter> adapters) : IToolExecutionGateway
{
    private readonly IReadOnlyList<IToolExecutionAdapter> _adapters = adapters.ToArray();

    public async Task<ToolExecutionResult> ExecuteAsync(
        Tool tool, JsonElement arguments, CancellationToken cancellationToken)
    {
        var matches = _adapters.Where(adapter => adapter.CanHandle(tool)).Take(2).ToArray();
        if (matches.Length == 0)
            return ToolExecutionResult.Fail("executor_unavailable", "No approved adapter is available for this tool.");
        if (matches.Length > 1)
            return ToolExecutionResult.Fail("ambiguous_executor", "Multiple adapters match this tool.");
        return await matches[0].ExecuteAsync(tool, arguments, cancellationToken);
    }
}
