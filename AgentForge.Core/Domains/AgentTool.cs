namespace AgentForge.Core.Domains;

/// <summary>Persisted allowlist membership; runtime must enforce it before tool execution.</summary>
public sealed class AgentTool
{
    public Guid AgentId { get; private set; }
    public Guid ToolId { get; private set; }
    private AgentTool() { }
    public AgentTool(Guid agentId, Guid toolId)
    {
        if (agentId == Guid.Empty || toolId == Guid.Empty) throw new ArgumentException("Agent and tool IDs are required.");
        AgentId = agentId;
        ToolId = toolId;
    }
}
