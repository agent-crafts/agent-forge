using Microsoft.Extensions.AI;

namespace AgentForge.Application.Tools.Registers;

/// <summary>Lists application-approved AI functions; catalog membership does not grant execution permission.</summary>
public interface IToolRegistry
{
    IReadOnlyList<AIFunction> GetTools();
    Task<IReadOnlyList<AIFunction>> GetToolsAsync(CancellationToken cancellationToken = default);
}
