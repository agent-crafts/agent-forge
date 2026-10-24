using AgentForge.Application.Tools.Commands;
using AgentForge.Application.Tools.Queries;
using AgentForge.Core.Domains;
using AgentForge.Core.Utilities;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[ApiController]
[Route("api/tools")]
public sealed class ToolsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateToolCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tool = await sender.Send(new GetToolByIdQuery(id), cancellationToken);
        return tool is null ? NotFound() : Ok(tool);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100)
            return BadRequest("Invalid pagination parameters.");

        var result = await sender.Send(
            new GetToolsPageQuery(new PagedQuery<Tool>(pageNumber, pageSize)),
            cancellationToken);
        return Ok(result);
    }
    [HttpPost("github-issue/try")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TryGitHubIssue(
        [FromForm] int issueNumber,
        [FromServices] AgentForge.Application.Tools.GitHub.GitHubTools githubTools,
        [FromServices] ILogger<ToolsController> logger,
        CancellationToken cancellationToken)
    {
        if (issueNumber <= 0)
            return BadRequest(new { success = false, error = new { code = "invalid_issue_number", message = "Issue number must be a positive integer." } });

        try
        {
            var result = await githubTools.GetGitHubIssueAsync(issueNumber, cancellationToken);
            return result.Success
                ? Ok(new { success = true, data = result.Data })
                : NotFound(new { success = false, error = result.Error });
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "GitHub issue lookup failed for issue {IssueNumber}", issueNumber);
            return StatusCode(502, new { success = false, error = new { code = "github_unavailable", message = "GitHub could not complete the lookup. Please try again." } });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(504, new { success = false, error = new { code = "github_timeout", message = "GitHub did not respond in time." } });
        }
    }

    // Compatibility model for legacy Razor views; not used by API actions.
    public sealed record ToolCatalogItem(string Name, string Description, string JsonSchema, string DisplayName, string Icon, string Category);
}
