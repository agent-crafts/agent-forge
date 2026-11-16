namespace AgentForge.Core.Domains;

public sealed class Project : Base
{
    private Project() { } // EF Core

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Icon { get; private set; } = string.Empty;
    public bool IsArchived { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Project Create(string name, string? description = null, string? icon = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200)
            throw new ArgumentException("Project name must contain 1–200 characters.", nameof(name));
        if (description?.Length > 2000)
            throw new ArgumentException("Project description must be at most 2000 characters.", nameof(description));

        var normalizedIcon = ValidateIcon(icon);
        var now = DateTimeOffset.UtcNow;
        return new Project
        {
            Name = name.Trim(),
            Description = description,
            Icon = normalizedIcon,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateDetails(string name, string? description, string? icon = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200)
            throw new ArgumentException("Project name must contain 1–200 characters.", nameof(name));
        if (description?.Length > 2000)
            throw new ArgumentException("Project description must be at most 2000 characters.", nameof(description));
        if (IsArchived) throw new InvalidOperationException("Archived projects cannot be edited.");
        Name = name.Trim();
        Description = description;
        Icon = ValidateIcon(icon);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string ValidateIcon(string? icon)
    {
        var normalized = icon?.Trim() ?? string.Empty;
        if (normalized.Length > 2048)
            throw new ArgumentException("Icon must be at most 2048 characters.", nameof(icon));
        if (normalized.Length > 0 && !System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^projects/[a-f0-9]{24}\.(png|jpg|webp|gif)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            throw new ArgumentException("Icon must reference an uploaded project image.", nameof(icon));
        return normalized;
    }

    public void Archive()
    {
        if (IsArchived) return;
        IsArchived = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
