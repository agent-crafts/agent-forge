namespace AgentForge.Core.Clients.GitHub;

public sealed record GitHubIssue(
    int Number,
    string Title,
    string State,
    string Url);
