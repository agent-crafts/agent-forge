namespace AgentForge.Core.Repositories;

/// <summary>Commits changes tracked by repositories in the current scope.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
