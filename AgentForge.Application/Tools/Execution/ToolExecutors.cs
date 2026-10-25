using System.Text.Json;

namespace AgentForge.Application.Tools.Execution;

/// <summary>Only explicitly registered application capabilities may execute.</summary>
public interface IToolExecutor
{
    string ToolName { get; }
    Task<ToolExecutionResult> ExecuteAsync(JsonElement input, CancellationToken cancellationToken);
}

public sealed record ToolExecutionResult(bool Success, object? Data, ToolError? Error)
{
    public static ToolExecutionResult Ok(object? data) => new(true, data, null);
    public static ToolExecutionResult Fail(string code, string message) => new(false, null, new ToolError(code, message));
}

public interface IToolExecutorRegistry
{
    bool CanExecute(string toolName);
    IToolExecutor? Resolve(string toolName);
}

public sealed class ToolExecutorRegistry(IEnumerable<IToolExecutor> executors) : IToolExecutorRegistry
{
    private readonly IReadOnlyDictionary<string, IToolExecutor> _executors =
        executors.ToDictionary(e => e.ToolName, StringComparer.Ordinal);

    public bool CanExecute(string toolName) => _executors.ContainsKey(toolName);
    public IToolExecutor? Resolve(string toolName) => _executors.GetValueOrDefault(toolName);
}

