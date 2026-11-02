using System.Text.Json;
using AgentForge.Core.Domains;

namespace AgentForge.Application.Tools.Execution;

/// <summary>Provider-independent execution entry point. Registration is not authorization.</summary>
public interface IToolExecutionGateway
{
    Task<ToolExecutionResult> ExecuteAsync(Tool tool, JsonElement arguments, CancellationToken cancellationToken);
}
