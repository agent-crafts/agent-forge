using System.Net;
using System.Net.Sockets;
using AgentForge.Core.Clients;
using AgentForge.Core.Clients.GitHub;
using AgentForge.Core.Clients.Http;
using AgentForge.Core.Services;
using AgentForge.Core.Repositories;
using AgentForge.Infrastructure.Clients;
using AgentForge.Infrastructure.Clients.GitHub;
using AgentForge.Infrastructure.Clients.Http;
using AgentForge.Infrastructure.Persistence;
using AgentForge.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AgentForge");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:AgentForge is required. Configure it using user-secrets or environment variables.");

        services.AddDbContext<AgentForgeDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IToolRepository, ToolRepository>();

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
        services.AddHttpClient<IHttpApiClient, HttpApiClient>(client => client.Timeout = TimeSpan.FromSeconds(10))
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false,
                UseProxy = false,
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                ConnectCallback = async (context, cancellationToken) =>
                {
                    var endpoint = context.DnsEndPoint;
                    var addresses = await Dns.GetHostAddressesAsync(endpoint.Host, cancellationToken);
                    var address = addresses.FirstOrDefault(PublicHttpUrlPolicy.IsPublicAddress)
                        ?? throw new HttpRequestException("The destination does not resolve to a public IP address.");
                    var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        await socket.ConnectAsync(new IPEndPoint(address, endpoint.Port), cancellationToken);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                }
            });
        return services;
    }
}
