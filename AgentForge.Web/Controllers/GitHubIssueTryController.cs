using System.Net;
using AgentForge.Application.Tools.GitHub;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

/// <summary>Explicit, read-only execution adapter for the GitHub issue Try Now UI.</summary>
[Route("api/tools/github-issue")]
public sealed class GitHubIssueTryController(GitHubTools githubTools, ILogger<GitHubIssueTryController> logger) : Controller
{
    [HttpPost("try")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Try([FromForm] int issueNumber, CancellationToken cancellationToken)
    {
        if (issueNumber <= 0)
            return BadRequest(new { success = false, error = new { code = "invalid_issue_number", message = "Issue number must be a positive integer." } });

        try
        {
            var result = await githubTools.GetGitHubIssueAsync(issueNumber, cancellationToken);
            if (!result.Success)
                return NotFound(new { success = false, error = result.Error });

            return Ok(new { success = true, data = result.Data });
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "GitHub issue lookup failed for issue {IssueNumber}", issueNumber);
            return StatusCode((int)HttpStatusCode.BadGateway,
                new { success = false, error = new { code = "github_unavailable", message = "GitHub could not complete the lookup. Please try again." } });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(504, new { success = false, error = new { code = "github_timeout", message = "GitHub did not respond in time." } });
        }
    }
}
