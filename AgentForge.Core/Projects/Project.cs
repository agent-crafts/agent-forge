namespace AgentForge.Core.Projects;

/// <summary>A project is the ownership boundary for future agents, tools and runs.</summary>
public sealed class Project
{
    public const int MaxNameLength = 120;
    public const int MaxDescriptionLength = 2000;

    private Project(Guid id, string name, string? description, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? ArchivedAt { get; private set; }
    public bool IsArchived => ArchivedAt.HasValue;

    public static Project Create(string name, string? description, DateTimeOffset now)
        => new(Guid.NewGuid(), NormalizeName(name), NormalizeDescription(description), now);

    public void UpdateDetails(string name, string? description, DateTimeOffset now)
    {
        EnsureActive();
        EnsureChronological(now);
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        UpdatedAt = now;
    }

    public void Archive(DateTimeOffset now)
    {
        EnsureActive();
        EnsureChronological(now);
        ArchivedAt = now;
        UpdatedAt = now;
    }

    private void EnsureActive()
    {
        if (IsArchived)
            throw new InvalidOperationException("Archived projects cannot be modified.");
    }

    private void EnsureChronological(DateTimeOffset now)
    {
        if (now < UpdatedAt)
            throw new ArgumentOutOfRangeException(nameof(now), "Update time cannot precede the last change.");
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required.", nameof(name));

        var normalized = name.Trim();
        if (normalized.Length > MaxNameLength)
            throw new ArgumentException($"Project name cannot exceed {MaxNameLength} characters.", nameof(name));

        return normalized;
    }

    private static string? NormalizeDescription(string? description)
    {
        var normalized = description?.Trim();
        if (string.IsNullOrEmpty(normalized))
            return null;
        if (normalized.Length > MaxDescriptionLength)
            throw new ArgumentException($"Project description cannot exceed {MaxDescriptionLength} characters.", nameof(description));
        return normalized;
    }
}
