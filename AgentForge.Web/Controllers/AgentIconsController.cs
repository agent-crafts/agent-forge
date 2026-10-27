using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

/// <summary>Stores validated, raster-only Agent icons in the app's static upload directory.</summary>
[ApiController]
[Route("api/agents/icons")]
public sealed class AgentIconsController(IWebHostEnvironment environment) : ControllerBase
{
    private const long MaxBytes = 2 * 1024 * 1024;
    private static readonly Dictionary<string, string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/png"] = ".png", ["image/jpeg"] = ".jpg",
        ["image/webp"] = ".webp", ["image/gif"] = ".gif"
    };

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxBytes + 16 * 1024)]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length is <= 0 or > MaxBytes)
            return BadRequest(new { error = "Select an image smaller than 2 MB." });

        if (!Extensions.TryGetValue(file.ContentType, out var extension))
            return BadRequest(new { error = "Only PNG, JPEG, WebP and GIF images are allowed." });

        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var read = await stream.ReadAsync(header, cancellationToken);
        var valid = extension switch
        {
            ".png" => read >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".jpg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".gif" => read >= 6 && (header.AsSpan(0, 6).SequenceEqual("GIF87a"u8) || header.AsSpan(0, 6).SequenceEqual("GIF89a"u8)),
            ".webp" => read >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
        if (!valid) return BadRequest(new { error = "The file does not match its image format." });

        var filename = Convert.ToHexString(RandomNumberGenerator.GetBytes(12)).ToLowerInvariant() + extension;
        var directory = Path.Combine(environment.WebRootPath, "uploads", "agent-icons");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, filename);
        stream.Position = 0;
        await using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            await stream.CopyToAsync(output, cancellationToken);

        return Ok(new { filename, url = "/uploads/agent-icons/" + filename });
    }
}
