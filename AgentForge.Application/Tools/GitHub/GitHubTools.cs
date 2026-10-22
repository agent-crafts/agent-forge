using AgentForge.Core.Clients.GitHub;

namespace AgentForge.Application.Tools.GitHub;

/// <summary>Agent-facing GitHub operations; delegates API access to the client contract.</summary>
public sealed class GitHubTools(IGitHubIssueClient githubIssueClient)
{
    public Task<GitHubIssue> GetGitHubIssueAsync(
        int issueNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(issueNumber);
        return githubIssueClient.GetIssueAsync(issueNumber, cancellationToken);
    }
}
