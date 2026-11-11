namespace AgentForge.Core.Clients;

public interface ILlmClient
{
    Task<LlmChatResponse> ChatAsync(LlmChatRequest request, CancellationToken token);

    // Retained for the existing /chat endpoint until it migrates to ChatAsync.
    Task<string?> GenerateAsync(string prompt, CancellationToken token);
}
