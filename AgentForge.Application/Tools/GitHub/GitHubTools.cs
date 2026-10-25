using System.Net;
using AgentForge.Core.Clients.GitHub;

namespace AgentForge.Application.Tools.GitHub;

/// <summary>Agent-facing GitHub operations; delegates API access to the client contract.</summary>
public sealed class GitHubTools(IGitHubIssueClient githubIssueClient)
{
    public async Task<ToolResult<GitHubIssue>> GetGitHubIssueAsync(
        int issueNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(issueNumber);

        try
        {
            var issue = await githubIssueClient.GetIssueAsync(issueNumber, cancellationToken);
            return ToolResult<GitHubIssue>.Ok(issue);
        }
        catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.UnprocessableEntity or HttpStatusCode.NotFound)
        {
            // A 404 may also conceal an inaccessible private repository/issue.
            return ToolResult<GitHubIssue>.Fail(
                "issue_not_found",
                "The requested GitHub issue was not found or is not accessible.");
        }
    }
}
