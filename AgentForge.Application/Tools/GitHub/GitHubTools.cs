using System.Net;
using AgentForge.Core.Clients.GitHub;

namespace AgentForge.Application.Tools.GitHub;

/// <summary>Result returned to an agent for a GitHub issue lookup.</summary>
public sealed record GitHubIssueToolResult(
    bool Success,
    GitHubIssue? Issue,
    GitHubIssueToolError? Error);

public sealed record GitHubIssueToolError(string Code, string Message);

/// <summary>Agent-facing GitHub operations; delegates API access to the client contract.</summary>
public sealed class GitHubTools(IGitHubIssueClient githubIssueClient)
{
    public async Task<GitHubIssueToolResult> GetGitHubIssueAsync(
        int issueNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(issueNumber);

        try
        {
            var issue = await githubIssueClient.GetIssueAsync(issueNumber, cancellationToken);
            return new GitHubIssueToolResult(true, issue, null);
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            // A 404 may also conceal an inaccessible private repository/issue.
            return new GitHubIssueToolResult(
                false,
                null,
                new GitHubIssueToolError(
                    "issue_not_found",
                    "The requested GitHub issue was not found or is not accessible."));
        }
    }
}
