using System.Text.Json;
using AgentForge.Application.Tools.GitHub;

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

/// <summary>Trusted, read-only GitHub issue capability; repository is fixed in the GitHub client.</summary>
public sealed class GitHubIssueExecutor(GitHubTools github) : IToolExecutor
{
    public string ToolName => "get_github_issue";

    public async Task<ToolExecutionResult> ExecuteAsync(JsonElement input, CancellationToken cancellationToken)
    {
        // The HTTP client is intentionally restricted to this repository.
        // Never silently ignore a different repository requested by the caller.
        if (!input.TryGetProperty("owner", out var owner) ||
            owner.ValueKind != JsonValueKind.String ||
            owner.GetString() != "agent-crafts" ||
            !input.TryGetProperty("repo", out var repo) ||
            repo.ValueKind != JsonValueKind.String ||
            repo.GetString() != "agent-forge")
            return ToolExecutionResult.Fail("unsupported_repository", "Only agent-crafts/agent-forge is supported.");

        if (!input.TryGetProperty("issue_number", out var number) ||
            number.ValueKind != JsonValueKind.Number || !number.TryGetInt32(out var issueNumber) || issueNumber <= 0)
            return ToolExecutionResult.Fail("invalid_issue_number", "issue_number must be a positive integer.");

        var result = await github.GetGitHubIssueAsync(issueNumber, cancellationToken);
        return result.Success
            ? ToolExecutionResult.Ok(result.Data)
            : ToolExecutionResult.Fail(result.Error?.Code ?? "execution_failed", result.Error?.Message ?? "Execution failed.");
    }
}
