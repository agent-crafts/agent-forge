namespace AgentForge.Core.Clients;

public interface ILlmClient
{
    Task<string?> GenerateAsync(string prompt, CancellationToken token);
}