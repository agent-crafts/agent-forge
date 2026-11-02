using Microsoft.Extensions.AI;

namespace AgentForge.Application.Tools.Registers.Http;

/// <summary>Creates an LLM-facing function for an explicitly configured, approved API endpoint.</summary>
public static class HttpApiToolSchemaFactory
{
    public static AIFunction CreateGetJson(HttpApiTools tools, string name, string description, Uri url)
    {
        ArgumentNullException.ThrowIfNull(tools);
        ArgumentNullException.ThrowIfNull(url);
        return AIFunctionFactory.Create(
            (CancellationToken cancellationToken) => tools.GetJsonAsync(url, cancellationToken),
            new AIFunctionFactoryOptions { Name = name, Description = description });
    }
}
