using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Projects.Commands;

public sealed record ArchiveProjectCommand(Guid Id) : ICommand<bool>;

public sealed class ArchiveProjectCommandHandler(IRepository<Project> projects, IUnitOfWork unitOfWork)
    : ICommandHandler<ArchiveProjectCommand, bool>
{
    public async ValueTask<bool> Handle(ArchiveProjectCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var project = await projects.GetByIdAsync(command.Id, cancellationToken);
        if (project is null || !project.IsActive || project.IsArchived) return false;
        project.Archive();
        projects.Update(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
