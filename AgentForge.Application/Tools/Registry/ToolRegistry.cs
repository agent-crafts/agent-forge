using System.Text.Json;
using AgentForge.Core.Clients.Http;
using AgentForge.Application.Tools.Registry.GitHub;
using AgentForge.Application.Tools.Registry.Http;
using AgentForge.Core.Enums;
using AgentForge.Core.Repositories;
using Microsoft.Extensions.AI;

namespace AgentForge.Application.Tools.Registry;

/// <summary>Application-approved LLM functions, including configured HTTP JSON tools from persistence.</summary>
public sealed class ToolRegistry(GitHubTools githubTools, HttpApiTools httpTools, IToolRepository repository) : IToolRegistry
{
    private readonly IReadOnlyList<AIFunction> _builtInTools =
        [GitHubToolSchemaFactory.CreateGetGitHubIssue(githubTools)];

    // Backwards-compatible built-in catalog. Use GetToolsAsync for persisted configured tools.
    public IReadOnlyList<AIFunction> GetTools() => _builtInTools;

    public async Task<IReadOnlyList<AIFunction>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        var functions = new List<AIFunction>(_builtInTools);
        var registeredNames = new HashSet<string>(functions.Select(f => f.Name), StringComparer.Ordinal);
        var tools = await repository.GetAllAsync(cancellationToken);
        foreach (var tool in tools)
        {
            if (!tool.IsActive || tool.Kind != ToolKind.Custom || tool.ExecutorKey != "http.get_json" ||
                !registeredNames.Add(tool.Name))
                continue;

            try
            {
                using var config = JsonDocument.Parse(tool.ConfigurationJson);
                if (config.RootElement.ValueKind != JsonValueKind.Object ||
                    !config.RootElement.TryGetProperty("url", out var value) || value.ValueKind != JsonValueKind.String ||
                    !Uri.TryCreate(value.GetString(), UriKind.Absolute, out var url) ||
                    !PublicHttpUrlPolicy.IsAllowed(url))
                    continue;

                functions.Add(HttpApiToolSchemaFactory.CreateGetJson(httpTools, tool.Name, tool.Description, url));
            }
            catch (JsonException)
            {
                // Invalid persisted configuration must not expose a callable function.
            }
        }
        return functions;
    }
}
