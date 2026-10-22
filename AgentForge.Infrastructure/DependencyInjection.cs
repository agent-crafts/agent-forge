using AgentForge.Core.Clients;
using AgentForge.Core.Services;
using AgentForge.Infrastructure.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ILlmClient, LlmClient>((provider, client) =>
        {
            var config = provider.GetService<IConfigService>();
            client.BaseAddress = new Uri(config.AI.BaseUrl);

            client.Timeout = TimeSpan.FromSeconds(config.AI.TimeoutSeconds ?? 0);
        });
        
        services.AddHttpClient<IGitHubIssueClient, GitHubIssueClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("AgentForge-ToolEngineeringLab/1.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        });

        //services.AddScoped<ILlmClient, LlmClient>();
        return services;
    }
}