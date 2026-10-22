using System.ComponentModel;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;

namespace AgentForge.Application.Tools.GitHub;

/// <summary>Defines the LLM-facing GitHub issue function and its public argument contract.</summary>
public static class GitHubToolSchemaFactory
{
    public static AIFunction CreateGetGitHubIssue(GitHubTools tools)
    {
        ArgumentNullException.ThrowIfNull(tools);

        // The adapter deliberately owns the external snake_case name.
        // The implementation remains GitHubTools.GetGitHubIssueAsync(int issueNumber, ...).
        return AIFunctionFactory.Create(
            ([Description("The positive GitHub issue number.")] int issue_number,
             CancellationToken cancellationToken) =>
                tools.GetGitHubIssueAsync(issue_number, cancellationToken),
            new AIFunctionFactoryOptions
            {
                Name = "get_github_issue",
                Description = "Get details of a GitHub issue by its number.",
                JsonSchemaCreateOptions = new AIJsonSchemaCreateOptions
                {
                    TransformSchemaNode = (context, schema) =>
                    {
                        // Only the integer issue-number argument receives this constraint.
                        if (context.TypeInfo.Type == typeof(int) && schema is JsonObject obj)
                        {
                            obj["minimum"] = 1;
                        }

                        return schema;
                    }
                }
            });
    }
}
