using AgentForge.Application.Agents.Runtime;
using AgentForge.Application.Chat.Commands;
using AgentForge.Application.Services;
using AgentForge.Application.Tools;
using AgentForge.Application.Tools.Registry;
using AgentForge.Application.Tools.Registry.GitHub;
using AgentForge.Application.Tools.Registry.Http;
using AgentForge.Application.Tools.Execution;
using AgentForge.Application.Tools.Execution.GitHub;
using AgentForge.Application.Tools.Execution.Http;
using AgentForge.Application.Tools.Execution.Mcp;
using AgentForge.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentForge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddScoped<IFileService, FileService>();

        services.AddScoped<IAgentRuntime, AgentRuntime>();
        services.AddScoped<ChatCommand>();
        services.AddScoped<GitHubTools>();
        services.AddScoped<HttpApiTools>();
        services.AddScoped<IToolRegistry, ToolRegistry>();
        services.AddScoped<IToolExecutor, GitHubIssueExecutor>();
        services.AddScoped<IToolExecutor, HttpGetJsonExecutor>();
        services.AddScoped<IToolExecutorRegistry, ToolExecutorRegistry>();
        services.AddScoped<IToolExecutionAdapter, CustomToolExecutionAdapter>();
        services.AddScoped<McpToolClient>();
        services.AddScoped<IToolExecutionAdapter, McpToolExecutionAdapter>();
        services.AddScoped<IToolExecutionGateway, ToolExecutionGateway>();

        return services;
    }
}
