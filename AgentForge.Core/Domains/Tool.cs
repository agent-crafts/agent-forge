using AgentForge.Core.Enums;

namespace AgentForge.Core.Domains;

/// <summary>Persisted tool metadata; not an executable tool or permission grant.</summary>
public sealed class Tool : Base
{
    private Tool() { }

    public string Name { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ToolKind Kind { get; private set; }
    public ToolCategory Category { get; private set; }
    public string? ImageUrl { get; private set; }
    public string JsonSchema { get; private set; } = "{}";

    public void UpdateDetails(string displayName, string description, ToolCategory category, string jsonSchema, string? imageUrl)
    {
        var validated = Create(Name, displayName, description, Kind, category, jsonSchema, imageUrl);
        DisplayName = validated.DisplayName;
        Description = validated.Description;
        Category = validated.Category;
        JsonSchema = validated.JsonSchema;
        ImageUrl = validated.ImageUrl;
    }

    public static Tool Create(string name, string displayName, string description, ToolKind kind,
        ToolCategory category, string jsonSchema, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 155 ||
            !System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-z][a-z0-9_]*$"))
            throw new ArgumentException("Tool name must be lowercase snake_case and at most 155 characters.", nameof(name));
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 155)
            throw new ArgumentException("Display name is required and must be at most 155 characters.", nameof(displayName));
        if (!Enum.IsDefined(kind) || !Enum.IsDefined(category))
            throw new ArgumentException("Invalid tool kind or category.");
        if (description is null || description.Length > 4000)
            throw new ArgumentException("Description must be at most 4000 characters.", nameof(description));
        if (string.IsNullOrWhiteSpace(jsonSchema) || jsonSchema.Length > 65536)
            throw new ArgumentException("Schema is too large.", nameof(jsonSchema));
        using var schema = System.Text.Json.JsonDocument.Parse(jsonSchema);
        if (schema.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object)
            throw new ArgumentException("Schema must be a JSON object.", nameof(jsonSchema));
        if (imageUrl is not null && !imageUrl.StartsWith("/api/files/", StringComparison.Ordinal) && !imageUrl.StartsWith("/uploads/tool-icons/", StringComparison.Ordinal) &&
            (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("Image URL must use HTTPS.", nameof(imageUrl));

        return new Tool
        {
            Name = name, DisplayName = displayName.Trim(), Description = description,
            Kind = kind, Category = category, JsonSchema = jsonSchema, ImageUrl = imageUrl
        };
    }
}
