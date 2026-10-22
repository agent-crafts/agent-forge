using AgentForge.Application.Tools.GitHub;
using Microsoft.Extensions.AI;

namespace AgentForge.Application.Tools;

/// <summary>Application-owned catalog of available AI functions.</summary>
public sealed class ToolRegistry(GitHubTools githubTools) : IToolRegistry
{
    private readonly IReadOnlyList<AIFunction> _tools =
        [GitHubToolSchemaFactory.CreateGetGitHubIssue(githubTools)];

    public IReadOnlyList<AIFunction> GetTools() => _tools;
}
