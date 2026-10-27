using AgentForge.Core.Domains;
using AgentForge.Core.Enums;
using AgentForge.Core.Repositories;
using AgentForge.Infrastructure.Persistence;
using AgentForge.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AgentForge.Tests;

/// <summary>Requires a migrated PostgreSQL instance; set AGENTFORGE_TEST_CONNECTION_STRING.</summary>
public sealed class AgentPersistenceTests
{
    [Fact]
    public async Task Agent_round_trips_through_repository_and_unit_of_work()
    {
        var connection = Environment.GetEnvironmentVariable("AGENTFORGE_TEST_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connection))
            return; // Integration environment not configured; PR verification must run with PostgreSQL.

        var options = new DbContextOptionsBuilder<AgentForgeDbContext>()
            .UseNpgsql(connection).Options;
        var id = Guid.Empty;
        try
        {
            await using (var context = new AgentForgeDbContext(options))
            {
                await context.Database.MigrateAsync();
                IRepository<Agent> repository = new Repository<Agent>(context);
                IUnitOfWork unit = new UnitOfWork(context);
                var agent = Agent.Create("Persistence test", "Round-trip", AgentCategory.Testing,
                    "Validate persistence", tags: ["qa", "database"]);
                id = agent.Id;
                await repository.AddAsync(agent);
                await unit.SaveChangesAsync();
            }

            await using (var context = new AgentForgeDbContext(options))
            {
                IRepository<Agent> repository = new Repository<Agent>(context);
                IUnitOfWork unit = new UnitOfWork(context);
                var agent = await repository.GetByIdAsync(id);
                Assert.NotNull(agent);
                Assert.Equal(AgentCategory.Testing, agent.Category);
                Assert.Equal(["qa", "database"], agent.Tags);
                agent.UpdateDetails("Updated", "Round-trip", AgentCategory.Development,
                    "Validate persistence");
                repository.Update(agent);
                await unit.SaveChangesAsync();
            }

            await using (var context = new AgentForgeDbContext(options))
            {
                IRepository<Agent> repository = new Repository<Agent>(context);
                var agent = await repository.GetByIdAsync(id);
                Assert.NotNull(agent);
                Assert.Equal("Updated", agent.Name);
                Assert.Equal(AgentCategory.Development, agent.Category);
            }
        }
        finally
        {
            if (id != Guid.Empty)
            {
                await using var cleanup = new AgentForgeDbContext(options);
                var agent = await cleanup.Set<Agent>().FindAsync(id);
                if (agent is not null)
                {
                    cleanup.Remove(agent);
                    await cleanup.SaveChangesAsync();
                }
            }
        }
    }
}
