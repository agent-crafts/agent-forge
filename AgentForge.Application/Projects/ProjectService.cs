using AgentForge.Core.Projects;

namespace AgentForge.Application.Projects;

/// <summary>Project use cases. Authentication and membership must be enforced before exposing these to HTTP.</summary>
public sealed class ProjectService(IProjectRepository repository, TimeProvider clock)
{
    public async Task<Project> CreateAsync(string name, string? description, CancellationToken cancellationToken = default)
    {
        var project = Project.Create(name, description, clock.GetUtcNow());
        await repository.AddAsync(project, cancellationToken);
        return project;
    }

    public Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default)
        => repository.ListAsync(cancellationToken);

    public Task<Project?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => repository.FindByIdAsync(id, cancellationToken);

    public async Task<Project?> UpdateAsync(Guid id, string name, string? description, CancellationToken cancellationToken = default)
    {
        var project = await repository.FindByIdAsync(id, cancellationToken);
        if (project is null) return null;
        project.UpdateDetails(name, description, clock.GetUtcNow());
        await repository.SaveAsync(project, cancellationToken);
        return project;
    }

    public async Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await repository.FindByIdAsync(id, cancellationToken);
        if (project is null) return false;
        project.Archive(clock.GetUtcNow());
        await repository.SaveAsync(project, cancellationToken);
        return true;
    }
}
