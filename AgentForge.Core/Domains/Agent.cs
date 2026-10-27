using AgentForge.Core.Enums;

namespace AgentForge.Core.Domains;

/// <summary>A reusable agent definition; does not grant execution permissions.</summary>
public sealed class Agent : Base
{
    private Agent() { } // EF Core

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AgentCategory Category { get; private set; }
    public string Icon { get; private set; } = "";
    public string[] Tags { get; private set; } = [];
    public string SystemInstructions { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Agent Create(string name, string description, AgentCategory category,
        string systemInstructions, string? icon = null, IEnumerable<string>? tags = null)
    {
        var details = Validate(name, description, category, systemInstructions, icon, tags);
        var now = DateTimeOffset.UtcNow;
        return new Agent
        {
            Name = details.Name,
            Description = details.Description,
            Category = details.Category,
            Icon = details.Icon,
            Tags = details.Tags,
            SystemInstructions = details.Instructions,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateDetails(string name, string description, AgentCategory category,
        string systemInstructions, string? icon = null, IEnumerable<string>? tags = null)
    {
        var details = Validate(name, description, category, systemInstructions, icon, tags);
        if (Name == details.Name && Description == details.Description && Category == details.Category &&
            Icon == details.Icon && Tags.SequenceEqual(details.Tags) && SystemInstructions == details.Instructions)
            return;

        Name = details.Name;
        Description = details.Description;
        Category = details.Category;
        Icon = details.Icon;
        Tags = details.Tags;
        SystemInstructions = details.Instructions;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // Base exposes nonvirtual Activate/Deactivate; these methods deliberately hide
    // them so calls through Agent maintain the audit timestamp.
    public new void Activate()
    {
        if (IsActive) return;
        base.Activate();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public new void Deactivate()
    {
        if (!IsActive) return;
        base.Deactivate();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static (string Name, string Description, AgentCategory Category, string Icon,
        string[] Tags, string Instructions) Validate(string name, string description,
        AgentCategory category, string systemInstructions, string? icon, IEnumerable<string>? tags)
    {
        var normalizedName = name?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName) || normalizedName.Length > 80)
            throw new ArgumentException("Name must contain 1–80 characters.", nameof(name));

        var normalizedDescription = description?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedDescription) || normalizedDescription.Length > 300)
            throw new ArgumentException("Description must contain 1–300 characters.", nameof(description));

        if (!Enum.IsDefined(category))
            throw new ArgumentOutOfRangeException(nameof(category));

        var normalizedIcon = string.IsNullOrWhiteSpace(icon) ? string.Empty : icon.Trim();
        if (normalizedIcon.Length > 32)
            throw new ArgumentException("Icon must be at most 32 characters.", nameof(icon));

        var normalizedInstructions = systemInstructions?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedInstructions) || normalizedInstructions.Length > 12000)
            throw new ArgumentException("System instructions must contain 1–12000 characters.", nameof(systemInstructions));

        var normalizedTags = (tags ?? [])
            .Select(tag => tag?.Trim() ?? string.Empty)
            .ToArray();
        if (normalizedTags.Any(tag => tag.Length is < 1 or > 40))
            throw new ArgumentException("Tags must contain 1–40 characters.", nameof(tags));
        normalizedTags = normalizedTags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (normalizedTags.Length > 10)
            throw new ArgumentException("At most 10 tags are allowed.", nameof(tags));

        return (normalizedName, normalizedDescription, category, normalizedIcon, normalizedTags, normalizedInstructions);
    }
}
