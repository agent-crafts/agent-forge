namespace AgentForge.Core.Domain;

/// <summary>Shared identity and active state for domain entities.</summary>
public abstract class BaseEntity
{
    protected BaseEntity(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty.", nameof(id));

        Id = id;
    }

    public Guid Id { get; }
    public bool IsActive { get; protected set; } = true;
}
