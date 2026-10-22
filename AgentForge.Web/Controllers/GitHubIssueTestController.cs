using System.Net;
using AgentForge.Core.Clients.GitHub;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

/// <summary>Development-only endpoint for testing the GitHub issue client.</summary>
[ApiController]
[Route("api/dev/github/issues")]
public sealed class GitHubIssueTestController(
    IGitHubIssueClient githubIssues,
    IWebHostEnvironment environment,
    ILogger<GitHubIssueTestController> logger) : ControllerBase
{
    [HttpGet("{issueNumber:int}")]
    public async Task<IActionResult> GetIssue(
        int issueNumber,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
            return NotFound();

        if (issueNumber <= 0)
            return BadRequest(new { error = "Issue number must be greater than zero." });

        try
        {
            var issue = await githubIssues.GetIssueAsync(issueNumber, cancellationToken);
            return Ok(issue);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound(new { error = "GitHub issue not found." });
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "GitHub issue lookup failed for issue {IssueNumber}", issueNumber);
            return StatusCode(502, new { error = "GitHub API request failed." });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return StatusCode(499);
        }
    }
}
