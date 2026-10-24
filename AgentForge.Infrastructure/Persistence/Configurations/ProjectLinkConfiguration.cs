using AgentForge.Core.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentForge.Infrastructure.Persistence.Configurations;

internal sealed class ProjectLinkConfiguration : IEntityTypeConfiguration<ProjectLink>
{
    public void Configure(EntityTypeBuilder<ProjectLink> builder)
    {
        builder.ToTable("project_links");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.ProjectId).HasColumnName("project_id").IsRequired();
        builder.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.ProjectId);
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Url).HasColumnName("url").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000);
    }
}
