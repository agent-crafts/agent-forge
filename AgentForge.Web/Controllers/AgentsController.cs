using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("agents")]
public sealed class AgentsController : Controller
{
    public sealed record AgentCard(string Id, string Name, string Description, string Category, string Icon, string[] Tags);

    private static readonly IReadOnlyList<AgentCard> SampleAgents =
    [
        new("planning", "Planning Agent", "Analyze requirements and create implementation plans.", "Planning", "◈", ["Requirements", "Architecture"]),
        new("coding", "Coding Agent", "Implement features, fix bugs, and refactor code.", "Development", "</>", ["Coding", "Debugging"]),
        new("review", "Review Agent", "Review code quality, security, and best practices.", "Quality", "◇", ["Code Review", "Security"]),
        new("qa", "QA Agent", "Design test cases and validate application behavior.", "Testing", "⚗", ["Testing", "Automation"]),
        new("devops", "DevOps Agent", "Assist with CI/CD and deployment workflows.", "Operations", "⚙", ["CI/CD", "Infrastructure"]),
        new("docs", "Documentation Agent", "Create and maintain technical documentation.", "Documentation", "▤", ["Writing", "Knowledge"]),
        new("research", "Research Agent", "Research technologies and summarize findings.", "Research", "⌕", ["Research", "Analysis"]),
        new("ui", "UI Agent", "Help design and implement user interfaces.", "Development", "▣", ["Frontend", "UX"]),
        new("data", "Data Agent", "Analyze data and prepare actionable reports.", "Data", "▥", ["Analytics", "Reporting"])
    ];

    [HttpGet("create")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Agent";
        ViewData["Section"] = "agents";
        return View();
    }

    [HttpGet("{id}")]
    public IActionResult Details(string id)
    {
        var agent = SampleAgents.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
        if (agent is null) return NotFound();

        ViewData["Title"] = agent.Name;
        ViewData["Section"] = "agents";
        return View(agent);
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Agents";
        ViewData["Section"] = "agents";
        return View(SampleAgents);
    }
}
