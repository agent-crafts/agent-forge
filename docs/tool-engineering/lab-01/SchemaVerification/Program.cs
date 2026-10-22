using System.Text.Json;
using AgentForge.Application.Tools.GitHub;
using AgentForge.Core.Clients.GitHub;

var tools = new GitHubTools(new StubGitHubIssueClient());
var function = GitHubToolSchemaFactory.CreateGetGitHubIssue(tools);
var schema = function.JsonSchema;

Console.WriteLine($"Tool name: {function.Name}");
Console.WriteLine("Generated JSON Schema:");
Console.WriteLine(JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true }));

if (function.Name != "get_github_issue")
    throw new Exception("Unexpected tool name.");

using var document = JsonDocument.Parse(JsonSerializer.Serialize(schema));
var root = document.RootElement;
var properties = root.GetProperty("properties");

if (!properties.TryGetProperty("issueNumber", out var issueNumber))
    throw new Exception("Expected C# parameter 'issueNumber' in schema.");

if (issueNumber.GetProperty("type").GetString() != "integer")
    throw new Exception("Expected integer issue number.");

if (properties.TryGetProperty("cancellationToken", out _))
    throw new Exception("CancellationToken should not be exposed to the model.");

Console.WriteLine("PASS: name, issueNumber type, and cancellationToken exclusion.");
Console.WriteLine($"minimum: {(issueNumber.TryGetProperty("minimum", out var minimum) ? minimum.ToString() : "not specified")}");

sealed class StubGitHubIssueClient : IGitHubIssueClient
{
    public Task<GitHubIssue> GetIssueAsync(int issueNumber, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Schema verification does not call GitHub.");
}
