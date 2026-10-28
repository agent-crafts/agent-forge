using AgentForge.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[ApiController]
[Route("api/files")]
public sealed class FilesController(IFileStorageService storage) : ControllerBase
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(2113536)]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null) return BadRequest(new { error = "Select an image smaller than 2 MB." });
        try
        {
            var saved = await storage.SaveImageAsync(file, cancellationToken);
            return Ok(new { filename = saved.Filename, url = saved.Url });
        }
        catch (ArgumentException error)
        {
            return BadRequest(new { error = error.Message });
        }
    }

    [HttpGet("{filename}")]
    public IActionResult Download(string filename)
    {
        var stored = storage.Find(filename);
        if (stored is null) return NotFound();
        return PhysicalFile(stored.Path, stored.ContentType);
    }
}
