using AgentForge.Core.Services;
using Microsoft.Extensions.Configuration;

namespace AgentForge.Application.Services;

public class ConfigService : IConfigService
{
    public AIConfig AI { get; }
    
    public ConfigService(IConfiguration configuration)
    {
        AI = PrepareAIConfig(configuration);
    }
    
    private AIConfig PrepareAIConfig(IConfiguration configuration)
    {
        return new AIConfig
        {
            Provider = configuration["AI:Provider"],
            BaseUrl = configuration["AI:BaseUrl"],
            Model = configuration["AI:Model"],
            Embedding = configuration["AI:Embedding"],
            TimeoutSeconds = configuration["AI:TimeoutSeconds"] != null 
                ? int.Parse(configuration["AI:TimeoutSeconds"] ?? "0") 
                : null
        };
    }

}