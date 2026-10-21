using AgentForge.Core.Projects;

namespace AgentForge.Application.Projects;

/// <summary>Persistence abstraction; implementations must enforce the caller's access scope.</summary>
public interface IProjectRepository
{
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    Task<Project?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(Project project, CancellationToken cancellationToken = default);
}
