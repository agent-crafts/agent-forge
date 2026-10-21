using AgentForge.Core.Clients;

namespace AgentForge.Application.Chat.Commands;

public sealed record SendMessageCommand(string Message);

public sealed record SendMessageResult(string Response);

public class ChatCommand(ILlmClient llmClient)
{
    public async Task<SendMessageResult> HandleAsync(
        SendMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        var response = await llmClient.GenerateAsync(
            command.Message,
            cancellationToken);

        return new SendMessageResult(response);
    }
}