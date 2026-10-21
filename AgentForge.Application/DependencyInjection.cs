using AgentForge.Application.Chat.Commands;
using AgentForge.Application.Services;
using AgentForge.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentForge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConfigService, ConfigService>();

        services.AddScoped<ChatCommand>();
        
        return services;
    }
}