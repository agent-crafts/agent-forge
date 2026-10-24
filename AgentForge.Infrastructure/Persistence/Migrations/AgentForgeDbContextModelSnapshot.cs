using AgentForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AgentForge.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AgentForgeDbContext))]
public partial class AgentForgeDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity("AgentForge.Core.Projects.Project", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uuid").HasColumnName("id");
            b.Property<string>("Name").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)").HasColumnName("name");
            b.Property<string>("Description").HasMaxLength(2000).HasColumnType("character varying(2000)").HasColumnName("description");
            b.Property<bool>("IsArchived").HasDefaultValue(false).HasColumnType("boolean").HasColumnName("is_archived");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone").HasColumnName("created_at");
            b.Property<DateTimeOffset>("UpdatedAt").HasColumnType("timestamp with time zone").HasColumnName("updated_at");
            b.HasKey("Id");
            b.HasIndex("CreatedAt").HasDatabaseName("ix_projects_created_at");
            b.ToTable("projects", t => t.HasCheckConstraint("ck_projects_name_not_blank", "length(btrim(name)) > 0"));
        });
    }
}
