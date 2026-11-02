using System.Text.Json;
using AgentForge.Application.Tools.GitHub;
using AgentForge.Application.Tools.Execution;

namespace AgentForge.Application.Tools.Execution.GitHub;

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
