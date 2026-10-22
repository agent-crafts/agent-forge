using Microsoft.Extensions.AI;

namespace AgentForge.Application.Tools.GitHub;

/// <summary>Defines the LLM-facing GitHub issue function from C# method metadata.</summary>
public static class GitHubToolSchemaFactory
{
    public static AIFunction CreateGetGitHubIssue(GitHubTools tools)
    {
        ArgumentNullException.ThrowIfNull(tools);

        return AIFunctionFactory.Create(
            tools.GetGitHubIssueAsync,
            name: "get_github_issue");
    }
}
