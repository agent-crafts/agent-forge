using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AgentForge.Infrastructure.Persistence;

// Design-time only. Environment variables override appsettings; never commit secrets.
public sealed class AgentForgeDbContextFactory : IDesignTimeDbContextFactory<AgentForgeDbContext>
{
    public AgentForgeDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("AgentForge");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Set ConnectionStrings__AgentForge to run EF Core tooling.");

        var options = new DbContextOptionsBuilder<AgentForgeDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(AgentForgeDbContext).Assembly.FullName))
            .Options;
        return new AgentForgeDbContext(options);
    }
}
