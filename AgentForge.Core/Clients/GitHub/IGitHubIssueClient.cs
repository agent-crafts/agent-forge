namespace AgentForge.Core.Clients.GitHub;

public interface IGitHubIssueClient
{
    Task<GitHubIssue> GetIssueAsync(
        string owner, string repository, int issueNumber,
        CancellationToken cancellationToken = default);
}
