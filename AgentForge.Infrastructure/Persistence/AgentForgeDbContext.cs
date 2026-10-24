using Microsoft.EntityFrameworkCore;

namespace AgentForge.Infrastructure.Persistence;

public sealed class AgentForgeDbContext(DbContextOptions<AgentForgeDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgentForgeDbContext).Assembly);
    }
}
