using System.Security.Cryptography;

namespace AgentForge.Web.Services;

public interface IFileStorageService
{
    Task<StoredFile> SaveImageAsync(IFormFile file, CancellationToken cancellationToken);
    StoredFile? Find(string filename);
}

public sealed record StoredFile(string Filename, string Url, string Path, string ContentType);

public sealed class LocalFileStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    private const long MaxBytes = 2 * 1024 * 1024;
    private static readonly Dictionary<string, string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/png"] = ".png", ["image/jpeg"] = ".jpg",
        ["image/webp"] = ".webp", ["image/gif"] = ".gif"
    };
    private static readonly Dictionary<string, string> MimeTypes = Extensions.ToDictionary(pair => pair.Value, pair => pair.Key);

    private string DirectoryPath => Path.Combine(environment.ContentRootPath, "file-uploads");

    public async Task<StoredFile> SaveImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length is <= 0 or > MaxBytes)
            throw new ArgumentException("Select an image smaller than 2 MB.");
        if (!Extensions.TryGetValue(file.ContentType, out var extension))
            throw new ArgumentException("Only PNG, JPEG, WebP and GIF images are allowed.");

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
        if (!valid) throw new ArgumentException("The file does not match its image format.");

        Directory.CreateDirectory(DirectoryPath);
        var filename = Convert.ToHexString(RandomNumberGenerator.GetBytes(12)).ToLowerInvariant() + extension;
        var stored = new StoredFile(filename, "/api/files/" + filename, Path.Combine(DirectoryPath, filename), file.ContentType);
        await using var input = file.OpenReadStream();
        await using var output = new FileStream(stored.Path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await input.CopyToAsync(output, cancellationToken);
        return stored;
    }

    public StoredFile? Find(string filename)
    {
        var extension = Path.GetExtension(filename);
        var stem = Path.GetFileNameWithoutExtension(filename);
        if (stem.Length != 24 || !stem.All(Uri.IsHexDigit) || !MimeTypes.TryGetValue(extension, out var mime))
            return null;
        var path = Path.Combine(DirectoryPath, filename);
        if (System.IO.File.Exists(path)) return new StoredFile(filename, "/api/files/" + filename, path, mime);
        var legacy = Path.Combine(environment.WebRootPath, "uploads", "agent-icons", filename);
        return System.IO.File.Exists(legacy) ? new StoredFile(filename, "/api/files/" + filename, legacy, mime) : null;
    }
}
