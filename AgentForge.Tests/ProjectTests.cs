using AgentForge.Core.Projects;
using Xunit;

namespace AgentForge.Tests;

public sealed class ProjectTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_NormalizesValuesAndAssignsIdentity()
    {
        var project = Project.Create("  Agent Studio  ", "  Prototype  ", Now);
        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.Equal("Agent Studio", project.Name);
        Assert.Equal("Prototype", project.Description);
        Assert.Equal(Now, project.CreatedAt);
        Assert.False(project.IsArchived);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_RejectsBlankName(string name)
        => Assert.Throws<ArgumentException>(() => Project.Create(name, null, Now));

    [Fact]
    public void Create_RejectsOversizedName()
        => Assert.Throws<ArgumentException>(() => Project.Create(new string('a', Project.MaxNameLength + 1), null, Now));

    [Fact]
    public void Archive_PreventsFurtherUpdates()
    {
        var project = Project.Create("Agent Studio", null, Now);
        project.Archive(Now.AddMinutes(1));
        Assert.True(project.IsArchived);
        Assert.Equal(Now.AddMinutes(1), project.ArchivedAt);
        Assert.Throws<InvalidOperationException>(() => project.UpdateDetails("Renamed", null, Now.AddMinutes(2)));
        Assert.Throws<InvalidOperationException>(() => project.Archive(Now.AddMinutes(2)));
    }

    [Fact]
    public void Update_RejectsEarlierTimestamp()
    {
        var project = Project.Create("Agent Studio", null, Now);
        Assert.Throws<ArgumentOutOfRangeException>(() => project.UpdateDetails("Renamed", null, Now.AddMinutes(-1)));
    }
}
