namespace AgentForge.Core.Domains;

/// <summary>Shared identity and activation state for persisted domain entities.</summary>
public abstract class Base
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public bool IsActive { get; protected set; } = true;

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
