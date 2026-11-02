using System.Text.Json;
using AgentForge.Application.Tools.Registry.GitHub;

namespace AgentForge.Application.Tools.Execution.GitHub;

/// <summary>Read-only GitHub issue executor; repository is supplied by persisted configuration.</summary>
public sealed class GitHubIssueExecutor(GitHubTools github) : IToolExecutor
{
    public string ToolName => "get_github_issue";

    public Task<ToolExecutionResult> ExecuteAsync(JsonElement input, CancellationToken cancellationToken)
        => ExecuteAsync(input, default, cancellationToken);

    public async Task<ToolExecutionResult> ExecuteAsync(
        JsonElement input, JsonElement configuration, CancellationToken cancellationToken)
    {
        if (input.ValueKind != JsonValueKind.Object ||
            !input.TryGetProperty("issue_number", out var number) ||
            number.ValueKind != JsonValueKind.Number ||
            !number.TryGetInt32(out var issueNumber) || issueNumber <= 0)
            return ToolExecutionResult.Fail("invalid_issue_number", "issue_number must be a positive integer.");

        var source = configuration;
        if (!source.TryGetProperty("owner", out var ownerValue) ||
            ownerValue.ValueKind != JsonValueKind.String ||
            !source.TryGetProperty("repo", out var repoValue) ||
            repoValue.ValueKind != JsonValueKind.String)
            return ToolExecutionResult.Fail("invalid_configuration", "GitHub owner and repository are required.");

        var owner = ownerValue.GetString()!;
        var repo = repoValue.GetString()!;
        if (!System.Text.RegularExpressions.Regex.IsMatch(owner, @"^[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,37}[a-zA-Z0-9])?$") ||
            !System.Text.RegularExpressions.Regex.IsMatch(repo, @"^[a-zA-Z0-9_.-]{1,100}$") ||
            repo is "." or "..")
            return ToolExecutionResult.Fail("invalid_configuration", "Invalid GitHub owner or repository.");

        var result = await github.GetGitHubIssueAsync(owner, repo, issueNumber, cancellationToken);
        return result.Success
            ? ToolExecutionResult.Ok(result.Data)
            : ToolExecutionResult.Fail(result.Error?.Code ?? "execution_failed", result.Error?.Message ?? "Execution failed.");
    }
}
