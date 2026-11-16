using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Projects.Commands;

public sealed record UpdateProjectCommand(Guid Id, string Name, string? Description, string? Icon = null) : ICommand<bool>;

public sealed class UpdateProjectCommandHandler(IRepository<Project> projects, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProjectCommand, bool>
{
    public async ValueTask<bool> Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var project = await projects.GetByIdAsync(command.Id, cancellationToken);
        if (project is null || !project.IsActive || project.IsArchived) return false;
        project.UpdateDetails(command.Name, command.Description, command.Icon);
        projects.Update(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
