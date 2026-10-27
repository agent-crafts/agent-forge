using AgentForge.Core.Domains;
using AgentForge.Core.Enums;
using Xunit;

namespace AgentForge.Tests;

public sealed class AgentTests
{
    private static Agent Create() => Agent.Create(
        " Code Reviewer ", " Reviews pull requests ", AgentCategory.Quality,
        " Review code for correctness. ", tags: [" Security ", "security", "Quality"]);

    [Fact]
    public void Create_normalizes_fields_and_initializes_identity_and_timestamps()
    {
        var agent = Create();
        Assert.NotEqual(Guid.Empty, agent.Id);
        Assert.True(agent.IsActive);
        Assert.Equal("Code Reviewer", agent.Name);
        Assert.Equal("Reviews pull requests", agent.Description);
        Assert.Equal("Review code for correctness.", agent.SystemInstructions);
        Assert.Equal(["Security", "Quality"], agent.Tags);
        Assert.Equal("◈", agent.Icon);
        Assert.Equal(agent.CreatedAt, agent.UpdatedAt);
        Assert.Equal(TimeSpan.Zero, agent.CreatedAt.Offset);
    }

    [Theory]
    [InlineData("", "description", "instructions")]
    [InlineData("name", "", "instructions")]
    [InlineData("name", "description", "")]
    public void Create_rejects_missing_required_fields(string name, string description, string instructions)
        => Assert.Throws<ArgumentException>(() =>
            Agent.Create(name, description, AgentCategory.Development, instructions));

    [Fact]
    public void Create_rejects_undefined_category_and_excess_tags()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Agent.Create("Name", "Description", (AgentCategory)999, "Instructions"));
        Assert.Throws<ArgumentException>(() =>
            Agent.Create("Name", "Description", AgentCategory.Data, "Instructions",
                tags: Enumerable.Range(1, 11).Select(x => $"tag{x}")));
    }

    [Fact]
    public void Update_rejects_invalid_values_without_mutating_existing_agent()
    {
        var agent = Create();
        var before = agent.UpdatedAt;
        Assert.Throws<ArgumentException>(() =>
            agent.UpdateDetails(" ", "Changed", AgentCategory.Data, "Changed"));
        Assert.Equal("Code Reviewer", agent.Name);
        Assert.Equal(AgentCategory.Quality, agent.Category);
        Assert.Equal(before, agent.UpdatedAt);
    }

    [Fact]
    public void Update_noop_preserves_timestamp()
    {
        var agent = Create();
        var before = agent.UpdatedAt;
        agent.UpdateDetails(agent.Name, agent.Description, agent.Category,
            agent.SystemInstructions, agent.Icon, agent.Tags);
        Assert.Equal(before, agent.UpdatedAt);
    }

    [Fact]
    public void Deactivation_and_reactivation_change_lifecycle_state()
    {
        var agent = Create();
        agent.Deactivate();
        Assert.False(agent.IsActive);
        Assert.True(agent.UpdatedAt >= agent.CreatedAt);
        agent.Activate();
        Assert.True(agent.IsActive);
        Assert.True(agent.UpdatedAt >= agent.CreatedAt);
    }

    [Fact]
    public void Boundary_limits_are_enforced()
    {
        Assert.Throws<ArgumentException>(() =>
            Agent.Create(new string('n', 81), "Description", AgentCategory.Other, "Instructions"));
        Assert.Throws<ArgumentException>(() =>
            Agent.Create("Name", new string('d', 301), AgentCategory.Other, "Instructions"));
        Assert.Throws<ArgumentException>(() =>
            Agent.Create("Name", "Description", AgentCategory.Other, new string('i', 12001)));
        Assert.Throws<ArgumentException>(() =>
            Agent.Create("Name", "Description", AgentCategory.Other, "Instructions", tags: [""]));
    }
}
