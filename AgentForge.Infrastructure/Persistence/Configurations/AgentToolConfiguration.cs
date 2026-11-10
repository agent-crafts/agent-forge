using AgentForge.Core.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentForge.Infrastructure.Persistence.Configurations;

internal sealed class AgentToolConfiguration : IEntityTypeConfiguration<AgentTool>
{
    public void Configure(EntityTypeBuilder<AgentTool> builder)
    {
        builder.ToTable("agent_tools");
        builder.HasKey(x => new { x.AgentId, x.ToolId });
        builder.Property(x => x.AgentId).HasColumnName("agent_id");
        builder.Property(x => x.ToolId).HasColumnName("tool_id");
        builder.HasOne<Agent>().WithMany().HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Tool>().WithMany().HasForeignKey(x => x.ToolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.ToolId).HasDatabaseName("ix_agent_tools_tool_id");
    }
}
