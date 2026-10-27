using AgentForge.Core.Domains;
using AgentForge.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentForge.Infrastructure.Persistence.Configurations;

internal sealed class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.ToTable("agents", table =>
        {
            table.HasCheckConstraint("ck_agents_name_not_blank", "length(btrim(name)) > 0");
            table.HasCheckConstraint("ck_agents_description_not_blank", "length(btrim(description)) > 0");
            table.HasCheckConstraint("ck_agents_instructions_valid", "length(btrim(system_instructions)) BETWEEN 1 AND 12000");
            table.HasCheckConstraint("ck_agents_category_valid",
                "category IN ('Development','Planning','Quality','Testing','Operations','Documentation','Research','Data','Other')");
            table.HasCheckConstraint("ck_agents_tags_count", "cardinality(tags) <= 10");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Category).HasColumnName("category").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Icon).HasColumnName("icon").HasMaxLength(32).IsRequired();
        builder.Property(x => x.Tags).HasColumnName("tags").HasColumnType("text[]").IsRequired();
        builder.Property(x => x.SystemInstructions).HasColumnName("system_instructions").HasColumnType("text").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(x => new { x.IsActive, x.CreatedAt, x.Id }).HasDatabaseName("ix_agents_active_created_id");
    }
}
