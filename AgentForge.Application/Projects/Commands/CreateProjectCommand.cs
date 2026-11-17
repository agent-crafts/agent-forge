using AgentForge.Core.Domains;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Projects.Commands;

public sealed record CreateProjectCommand(string Name, string? Description, string? Icon = null) : ICommand<Guid>;

public sealed class CreateProjectCommandHandler(IRepository<Project> projects, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProjectCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var project = Project.Create(command.Name, command.Description, command.Icon);
        await projects.AddAsync(project, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return project.Id;
    }
}
