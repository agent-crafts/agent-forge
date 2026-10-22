using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace AgentForge.Application.Tools.GitHub;

/// <summary>Defines the LLM-facing GitHub issue function from C# metadata.</summary>
public static class GitHubToolSchemaFactory
{
    public static AIFunction CreateGetGitHubIssue(GitHubTools tools)
    {
        ArgumentNullException.ThrowIfNull(tools);

        return AIFunctionFactory.Create(
            ([Description("The GitHub issue number.")] int issue_number,
             CancellationToken cancellationToken) =>
                tools.GetGitHubIssueAsync(issue_number, cancellationToken),
            name: "get_github_issue",
            description: "Get details of a GitHub issue by its number.");
    }
}
