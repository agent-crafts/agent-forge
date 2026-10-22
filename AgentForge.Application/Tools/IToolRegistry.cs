using Microsoft.Extensions.AI;

namespace AgentForge.Application.Tools;

/// <summary>Lists tools registered by the application; does not grant execution permission.</summary>
public interface IToolRegistry
{
    IReadOnlyList<AIFunction> GetTools();
}
