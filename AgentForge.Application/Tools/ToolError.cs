namespace AgentForge.Application.Tools;

/// <summary>Safe, machine-readable error returned by an agent tool.</summary>
public sealed record ToolError(string Code, string Message);
