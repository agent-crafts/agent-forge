using AgentForge.Infrastructure.Persistence;
using AgentForge.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AgentForge.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AgentForgeDbContext))]
public partial class AgentForgeDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity("AgentForge.Core.Domains.Project", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uuid").HasColumnName("id");
            b.Property<string>("Name").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)").HasColumnName("name");
            b.Property<string>("Description").HasMaxLength(2000).HasColumnType("character varying(2000)").HasColumnName("description");
            b.Property<bool>("IsArchived").HasDefaultValue(false).HasColumnType("boolean").HasColumnName("is_archived");
            b.Property<bool>("IsActive").HasDefaultValue(true).HasColumnType("boolean").HasColumnName("is_active");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone").HasColumnName("created_at");
            b.Property<DateTimeOffset>("UpdatedAt").HasColumnType("timestamp with time zone").HasColumnName("updated_at");
            b.HasKey("Id");
            b.HasIndex("CreatedAt").HasDatabaseName("ix_projects_created_at");
            b.ToTable("projects", t => t.HasCheckConstraint("ck_projects_name_not_blank", "length(btrim(name)) > 0"));
        });
        modelBuilder.Entity("AgentForge.Core.Domains.Tool", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uuid").HasColumnName("id");
            b.Property<bool>("IsActive").HasDefaultValue(true).HasColumnType("boolean").HasColumnName("is_active");
            b.Property<string>("Name").IsRequired().HasMaxLength(155).HasColumnType("character varying(155)").HasColumnName("name");
            b.Property<string>("DisplayName").IsRequired().HasMaxLength(155).HasColumnType("character varying(155)").HasColumnName("display_name");
            b.Property<string>("Description").IsRequired().HasMaxLength(4000).HasColumnType("character varying(4000)").HasColumnName("description");
            b.Property<ToolKind>("Kind").HasConversion<string>().IsRequired().HasMaxLength(32).HasColumnType("character varying(32)").HasColumnName("kind");
            b.Property<ToolCategory>("Category").HasConversion<string>().IsRequired().HasMaxLength(32).HasColumnType("character varying(32)").HasColumnName("category");
            b.Property<string>("ImageUrl").HasMaxLength(2048).HasColumnType("character varying(2048)").HasColumnName("image_url");
            b.Property<string>("JsonSchema").IsRequired().HasColumnType("jsonb").HasColumnName("json_schema");
            b.HasKey("Id");
            b.HasIndex("Name").IsUnique();
            b.ToTable("tools");
        });

    }
}
