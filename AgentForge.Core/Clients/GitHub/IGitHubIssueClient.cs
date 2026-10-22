namespace AgentForge.Core.Clients.GitHub;

public interface IGitHubIssueClient
{
    Task<GitHubIssue> GetIssueAsync(
        int issueNumber,
        CancellationToken cancellationToken = default);
}
