namespace AgentForge.Core.Services;

public interface IConfigService
{
    public AIConfig AI { get; }
}

public class AIConfig
{
    public string? Provider { get; set; }
    public string? BaseUrl { get; set; }
    public string? Model { get; set; }
    public string? Embedding { get; set; }
    public int? TimeoutSeconds { get; set; }
}