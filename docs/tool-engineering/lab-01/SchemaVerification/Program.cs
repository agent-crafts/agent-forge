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
if (stub.LastIssueNumber != 42 || stub.CallCount != 1)
    throw new Exception("Runtime argument mapping failed.");

Console.WriteLine("PASS: issue_number=42 maps to GitHubTools issueNumber at runtime.");

await ExpectRejectedAsync("zero", new AIFunctionArguments { ["issue_number"] = 0 });
await ExpectRejectedAsync("negative", new AIFunctionArguments { ["issue_number"] = -5 });
await ExpectRejectedAsync("wrong type", new AIFunctionArguments { ["issue_number"] = "hello" });

Console.WriteLine("PASS: invalid arguments did not reach GitHub client.");

stub.ReturnNotFound = true;
var missingResult = await function.InvokeAsync(new AIFunctionArguments { ["issue_number"] = 999999 });
var missingJson = JsonSerializer.Serialize(missingResult);
using (var missingDocument = JsonDocument.Parse(missingJson))
{
    var result = missingDocument.RootElement;
    // AIFunction invocation may wrap results; serialize the result for inspection.
    if (!missingJson.Contains("issue_not_found", StringComparison.Ordinal) ||
        !missingJson.Contains("not found or is not accessible", StringComparison.Ordinal))
        throw new Exception($"404 did not produce a safe tool error: {missingJson}");
}
Console.WriteLine("PASS: 404 returns a safe issue_not_found result.");

stub.ReturnNotFound = false;
stub.ReturnServerError = true;
var callsBeforeServerError = stub.CallCount;
try
{
    await function.InvokeAsync(new AIFunctionArguments { ["issue_number"] = 500 });
    throw new Exception("500 was incorrectly returned as a successful tool result.");
}
catch (HttpRequestException exception) when (exception.StatusCode == System.Net.HttpStatusCode.InternalServerError)
{
    if (stub.CallCount != callsBeforeServerError + 1)
        throw new Exception("Expected the GitHub client to be called exactly once for 500.");

    Console.WriteLine("PASS: 500 propagates as HttpRequestException (not issue_not_found).");
}



async Task ExpectRejectedAsync(string scenario, AIFunctionArguments arguments)
{
    var callsBefore = stub.CallCount;
    try
    {
        await function.InvokeAsync(arguments);
    }
    catch (Exception exception)
    {
        if (stub.CallCount != callsBefore)
            throw new Exception($"{scenario}: GitHub client was called despite invalid input.", exception);

        Console.WriteLine($"PASS: {scenario} rejected ({exception.GetType().Name}).");
        return;
    }

    throw new Exception($"{scenario}: invalid argument was accepted.");
}

sealed class StubGitHubIssueClient : IGitHubIssueClient
{
    public int? LastIssueNumber { get; private set; }
    public int CallCount { get; private set; }
    public bool ReturnNotFound { get; set; }
    public bool ReturnServerError { get; set; }

    public Task<GitHubIssue> GetIssueAsync(int issueNumber, CancellationToken cancellationToken = default)
    {
        LastIssueNumber = issueNumber;
        CallCount++;
        if (ReturnNotFound)
            throw new HttpRequestException("GitHub returned 404.", null, System.Net.HttpStatusCode.NotFound);
        if (ReturnServerError)
            throw new HttpRequestException("GitHub returned 500.", null, System.Net.HttpStatusCode.InternalServerError);
        return Task.FromResult(new GitHubIssue(issueNumber, "Test", "open", "https://example.invalid/issue/42"));
    }
}
