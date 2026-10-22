namespace AgentForge.Core.Clients;

public interface IGitHubIssueClient
{
    Task<GitHubIssue> GetIssueAsync(
        int issueNumber,
        CancellationToken cancellationToken = default);
}
