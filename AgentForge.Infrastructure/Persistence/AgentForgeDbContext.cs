using AgentForge.Core.Domains;
using Microsoft.EntityFrameworkCore;

namespace AgentForge.Infrastructure.Persistence;

public sealed class AgentForgeDbContext(DbContextOptions<AgentForgeDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgentForgeDbContext).Assembly);
    }
}
