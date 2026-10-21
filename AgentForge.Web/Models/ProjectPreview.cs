namespace AgentForge.Web.Models;

/// <summary>Demo-only project data. Not persisted or connected to the domain service.</summary>
public sealed record ProjectPreview(string Slug, string Name, string Description, string Status, string Initials, string Color, int Agents, int Documents, int Runs, string JiraUrl, string GitHubUrl, string DocumentationUrl);

public static class ProjectPreviewData
{
    public static readonly IReadOnlyList<ProjectPreview> Projects = new[]
    {
        new ProjectPreview("ecommerce", "E-Commerce Platform", "AI-powered commerce with personalized recommendations and analytics.", "Active", "EC", "#7155ef", 5, 24, 56, "https://example.atlassian.net/browse/ECOM", "https://github.com/example/ecommerce", "https://docs.example.com/ecommerce"),
        new ProjectPreview("banking", "Mobile Banking App", "Secure mobile banking experience and customer onboarding.", "Active", "MB", "#2477e6", 3, 12, 21, "https://example.atlassian.net/browse/BANK", "https://github.com/example/mobile-banking", "https://docs.example.com/banking"),
        new ProjectPreview("internal-tools", "Internal Tools", "Developer productivity tools and internal automation.", "Active", "IT", "#099b98", 2, 8, 14, "", "https://github.com/example/internal-tools", ""),
        new ProjectPreview("research", "AI Research", "Experimentation workspace for agent capabilities.", "Archived", "AI", "#66768e", 4, 17, 32, "", "", "")
    };

    public static ProjectPreview? Find(string slug) => Projects.FirstOrDefault(p => string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase));
}
