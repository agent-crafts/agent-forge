using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AgentForge.Infrastructure.Persistence;

// Design-time only: supply a connection string via environment variables; never commit secrets.
public sealed class AgentForgeDbContextFactory : IDesignTimeDbContextFactory<AgentForgeDbContext>
{
    public AgentForgeDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__AgentForge")
            ?? throw new InvalidOperationException("Set ConnectionStrings__AgentForge to run EF Core tooling.");
        var options = new DbContextOptionsBuilder<AgentForgeDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(AgentForgeDbContext).Assembly.FullName))
            .Options;
        return new AgentForgeDbContext(options);
    }
}
