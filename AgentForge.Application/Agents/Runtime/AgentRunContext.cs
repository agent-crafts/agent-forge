using AgentForge.Core.Clients;

namespace AgentForge.Application.Agents.Runtime;

// A context belongs to one execution, never to the registered runtime service.
internal sealed class AgentRunContext(Guid agentId)
{
    private readonly List<LlmMessage> _messages = [];
    private readonly List<AgentRunTraceEvent> _trace = [];

    public Guid RunId { get; } = Guid.NewGuid();
    public Guid AgentId { get; } = agentId;
    public DateTimeOffset StartedAt { get; } = DateTimeOffset.UtcNow;
    public int InferenceCount { get; private set; }
    public IReadOnlyList<LlmMessage> Messages => _messages;
    public IReadOnlyList<AgentRunTraceEvent> Trace => _trace;

    public void AddMessage(LlmMessage message) => _messages.Add(message);
    public int NextInference() => ++InferenceCount;

    public void Record(string type, string status, object details, long durationMs = 0) =>
        _trace.Add(new AgentRunTraceEvent(type, status, DateTimeOffset.UtcNow, durationMs, details));
}
