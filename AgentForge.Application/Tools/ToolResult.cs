namespace AgentForge.Application.Tools;

/// <summary>Provider-independent result of executing an agent tool.</summary>
public sealed record ToolResult<T>(
    bool Success,
    T? Data,
    ToolError? Error)
{
    public static ToolResult<T> Ok(T data) => new(true, data, null);

    public static ToolResult<T> Fail(string code, string message) =>
        new(false, default, new ToolError(code, message));
}

/// <summary>Safe, machine-readable error returned by an agent tool.</summary>
public sealed record ToolError(string Code, string Message);
