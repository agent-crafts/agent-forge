namespace AgentForge.Application.Tools.Execution;

public sealed class ToolExecutorRegistry(IEnumerable<IToolExecutor> executors) : IToolExecutorRegistry
{
    private readonly IReadOnlyDictionary<string, IToolExecutor> _executors =
        executors.ToDictionary(e => e.ToolName, StringComparer.Ordinal);

    public bool CanExecute(string toolName) => _executors.ContainsKey(toolName);
    public IToolExecutor? Resolve(string toolName) => _executors.GetValueOrDefault(toolName);
}
