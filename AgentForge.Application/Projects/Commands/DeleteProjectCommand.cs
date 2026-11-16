using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Projects.Commands;

/// <summary>Soft-deletes a project by deactivating it; persisted data and files remain intact.</summary>
public sealed record DeleteProjectCommand(Guid Id) : ICommand<bool>;

public sealed class DeleteProjectCommandHandler(IRepository<Project> projects, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProjectCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var project = await projects.GetByIdAsync(command.Id, cancellationToken);
        if (project is null || !project.IsActive) return false;

        project.Deactivate();
        projects.Update(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
