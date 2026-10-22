namespace AgentForge.Core.Clients;

public sealed record GitHubIssue(
    int Number,
    string Title,
    string State,
    string Url);
