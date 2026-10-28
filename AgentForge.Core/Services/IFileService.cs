namespace AgentForge.Core.Services;

public sealed record StoredFile(string Filename, string Url, string Path, string ContentType);

public interface IFileService
{
    Task<StoredFile> SaveImageAsync(Stream content, long length, string contentType, CancellationToken cancellationToken);
    StoredFile? Find(string filename);
}
