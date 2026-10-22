using System.ComponentModel;
using AgentForge.Core.Clients.GitHub;

namespace AgentForge.Application.Tools.GitHub;

/// <summary>Agent-facing GitHub operations; delegates API access to the client contract.</summary>
public sealed class GitHubTools(IGitHubIssueClient githubIssueClient)
{
    [Description("Get details of a GitHub issue by its number.")]
    public Task<GitHubIssue> GetGitHubIssueAsync(
        [Description("The positive GitHub issue number.")] int issueNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(issueNumber);
        return githubIssueClient.GetIssueAsync(issueNumber, cancellationToken);
    }
}
