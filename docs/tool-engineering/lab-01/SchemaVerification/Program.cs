using System.Text.Json;
using AgentForge.Application.Tools.GitHub;
using AgentForge.Core.Clients.GitHub;
using Microsoft.Extensions.AI;

var stub = new StubGitHubIssueClient();
var tools = new GitHubTools(stub);
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

if (!properties.TryGetProperty("issue_number", out var issueNumber))
    throw new Exception("Expected public parameter 'issue_number' in schema.");

if (properties.TryGetProperty("issueNumber", out _) ||
    properties.TryGetProperty("cancellationToken", out _))
    throw new Exception("Internal parameter names must not be exposed.");

if (issueNumber.GetProperty("type").GetString() != "integer" ||
    issueNumber.GetProperty("minimum").GetInt32() != 1)
    throw new Exception("Expected integer issue_number with minimum 1.");

if (!root.GetProperty("required").EnumerateArray()
        .Any(item => item.GetString() == "issue_number"))
    throw new Exception("issue_number must be required.");

Console.WriteLine("PASS: public name, type, required, minimum and cancellationToken exclusion.");

await function.InvokeAsync(new AIFunctionArguments { ["issue_number"] = 42 });
if (stub.LastIssueNumber != 42)
    throw new Exception("Runtime argument mapping failed.");

Console.WriteLine("PASS: issue_number maps to GitHubTools issueNumber at runtime.");

sealed class StubGitHubIssueClient : IGitHubIssueClient
{
    public int? LastIssueNumber { get; private set; }

    public Task<GitHubIssue> GetIssueAsync(int issueNumber, CancellationToken cancellationToken = default)
    {
        LastIssueNumber = issueNumber;
        return Task.FromResult(new GitHubIssue(issueNumber, "Test", "open", "https://example.invalid/issue/42"));
    }
}
