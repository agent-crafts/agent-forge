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
                    !HttpUrlTemplate.TryResolve(value.GetString(), JsonSerializer.SerializeToElement(new Dictionary<string, object>()), out var url) &&
                    !HttpUrlTemplate.TryResolve(value.GetString(), JsonSerializer.SerializeToElement(ExtractTemplateSample(value.GetString()!)), out url))
                    continue;

                // Parameterized templates are executed through the validated ToolExecutionGateway.\n                // Do not advertise them as zero-argument LLM functions.\n                if (!value.GetString()!.Contains('{'))\n                    functions.Add(HttpApiToolSchemaFactory.CreateGetJson(httpTools, tool.Name, tool.Description, url!));
            }
            catch (JsonException)
            {
                // Invalid persisted configuration must not expose a callable function.
            }
        }
        return functions;
    }

    private static Dictionary<string, object> ExtractTemplateSample(string template)
    {
        var result = new Dictionary<string, object>();
        foreach (System.Text.RegularExpressions.Match match in
                 System.Text.RegularExpressions.Regex.Matches(template, @"\\{([a-z][a-z0-9_]*)\\}"))
            result[match.Groups[1].Value] = 1;
        return result;
    }
}
