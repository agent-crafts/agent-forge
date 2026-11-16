using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using AgentForge.Core.Utilities;
using Mediator;

namespace AgentForge.Application.Projects;

public sealed record CreateProjectCommand(string Name, string? Description) : ICommand<Guid>;
public sealed class CreateProjectHandler(IRepository<Project> projects, IUnitOfWork unitOfWork) : ICommandHandler<CreateProjectCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateProjectCommand command, CancellationToken ct)
    {
        var project = Project.Create(command.Name, command.Description);
        await projects.AddAsync(project, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return project.Id;
    }
}

public sealed record UpdateProjectCommand(Guid Id, string Name, string? Description) : ICommand<bool>;
public sealed class UpdateProjectHandler(IRepository<Project> projects, IUnitOfWork unitOfWork) : ICommandHandler<UpdateProjectCommand, bool>
{
    public async ValueTask<bool> Handle(UpdateProjectCommand command, CancellationToken ct)
    {
        var project = await projects.GetByIdAsync(command.Id, ct);
        if (project is null || project.IsArchived) return false;
        project.UpdateDetails(command.Name, command.Description);
        projects.Update(project);
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }
}

public sealed record ArchiveProjectCommand(Guid Id) : ICommand<bool>;
public sealed class ArchiveProjectHandler(IRepository<Project> projects, IUnitOfWork unitOfWork) : ICommandHandler<ArchiveProjectCommand, bool>
{
    public async ValueTask<bool> Handle(ArchiveProjectCommand command, CancellationToken ct)
    {
        var project = await projects.GetByIdAsync(command.Id, ct);
        if (project is null || project.IsArchived) return false;
        project.Archive();
        projects.Update(project);
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }
}

public sealed record GetProjectByIdQuery(Guid Id) : IQuery<Project?>;
public sealed class GetProjectByIdHandler(IRepository<Project> projects) : IQueryHandler<GetProjectByIdQuery, Project?>
{
    public async ValueTask<Project?> Handle(GetProjectByIdQuery query, CancellationToken ct)
        => await projects.GetByIdAsync(query.Id, ct);
}

public sealed record GetProjectsPageQuery(int PageNumber, int PageSize) : IQuery<PagedResult<Project>>;
public sealed class GetProjectsPageHandler(IRepository<Project> projects) : IQueryHandler<GetProjectsPageQuery, PagedResult<Project>>
{
    public async ValueTask<PagedResult<Project>> Handle(GetProjectsPageQuery query, CancellationToken ct)
        => await projects.GetPagedAsync(new PagedQuery<Project>(query.PageNumber, query.PageSize,
            p => !p.IsArchived && p.IsActive, p => p.OrderBy(x => x.Name)), ct);
}
