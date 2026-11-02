using AgentForge.Application.Tools.Registers;
namespace AgentForge.Application.Tools.Execution;

public sealed record ToolExecutionResult(bool Success, object? Data, ToolError? Error)
{
    public static ToolExecutionResult Ok(object? data) => new(true, data, null);
    public static ToolExecutionResult Fail(string code, string message) => new(false, null, new ToolError(code, message));
}
