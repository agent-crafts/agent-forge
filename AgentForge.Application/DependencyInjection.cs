using AgentForge.Application.Chat.Commands;
using AgentForge.Application.Tools.Commands;
using AgentForge.Application.Tools.Queries;
using AgentForge.Application.Services;
using AgentForge.Application.Tools;
using AgentForge.Application.Tools.GitHub;
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
        services.AddScoped<GitHubTools>();
        services.AddScoped<IToolRegistry, ToolRegistry>();
        services.AddScoped<CreateToolCommandHandler>();
        services.AddScoped<GetToolByIdQueryHandler>();
        services.AddScoped<GetToolsPageQueryHandler>();

        return services;
    }
}
