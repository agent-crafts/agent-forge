namespace AgentForge.Application.Tools.Execution;

public interface IToolExecutorRegistry
{
    bool CanExecute(string toolName);
    IToolExecutor? Resolve(string toolName);
}
