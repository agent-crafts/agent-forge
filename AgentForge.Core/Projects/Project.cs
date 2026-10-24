namespace AgentForge.Core.Projects;

public sealed class Project
{
    private Project() { } // EF Core

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Project Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200)
            throw new ArgumentException("Project name must contain 1–200 characters.", nameof(name));
        if (description?.Length > 2000)
            throw new ArgumentException("Project description must be at most 2000 characters.", nameof(description));

        var now = DateTimeOffset.UtcNow;
        return new Project
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Archive()
    {
        if (IsArchived) return;
        IsArchived = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
