namespace AgentForge.Core.Domains;

public sealed class ProjectLink : Base
{
    private ProjectLink() { }

    public Guid ProjectId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string Type { get; private set; } = "Other";
    public string? Description { get; private set; }

    public static ProjectLink Create(Guid projectId, string title, string url, string type = "Other", string? description = null)
    {
        if (projectId == Guid.Empty) throw new ArgumentException("Project ID is required.", nameof(projectId));
        if (string.IsNullOrWhiteSpace(title) || title.Length > 120)
            throw new ArgumentException("Title must contain 1–120 characters.", nameof(title));
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) || url.Length > 2048)
            throw new ArgumentException("URL must be an absolute HTTP(S) URL.", nameof(url));
        if (string.IsNullOrWhiteSpace(type) || type.Length > 50)
            throw new ArgumentException("Type must contain 1–50 characters.", nameof(type));
        if (description?.Length > 1000)
            throw new ArgumentException("Description is too long.", nameof(description));
        return new ProjectLink { ProjectId = projectId, Title = title.Trim(), Url = url, Type = type, Description = description };
    }
}
