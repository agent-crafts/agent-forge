using System.Net.Http.Json;
using System.Net;
using System.Text.Json.Serialization;
using AgentForge.Core.Clients.GitHub;

namespace AgentForge.Infrastructure.Clients.GitHub;

public sealed class GitHubIssueClient(HttpClient client) : IGitHubIssueClient
{
    public async Task<GitHubIssue> GetIssueAsync(
        int issueNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(issueNumber);

        using var response = await client.GetAsync(
            $"repos/agent-crafts/agent-forge/issues/{issueNumber}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var issue = await response.Content.ReadFromJsonAsync<GitHubIssueResponse>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("GitHub returned an empty issue response.");

        if (issue.PullRequest is not null)
            throw new HttpRequestException("The requested item is a pull request, not an issue.", null, HttpStatusCode.UnprocessableEntity);

        return new GitHubIssue(issue.Number, issue.Title, issue.State, issue.HtmlUrl);
    }

    private sealed record GitHubIssueResponse(
        [property: JsonPropertyName("number")] int Number,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("state")] string State,
        [property: JsonPropertyName("html_url")] string HtmlUrl,
        [property: JsonPropertyName("pull_request")] System.Text.Json.JsonElement? PullRequest);
}
