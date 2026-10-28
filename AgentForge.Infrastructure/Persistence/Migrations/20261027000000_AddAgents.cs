using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AgentForge.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AgentForgeDbContext))]
[Migration("20261027000000_AddAgents")]
public partial class AddAgents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "agents",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                icon = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                tags = table.Column<string[]>(type: "text[]", nullable: false),
                system_instructions = table.Column<string>(type: "text", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_agents", x => x.id);
                table.CheckConstraint("ck_agents_name_not_blank", "length(btrim(name)) > 0");
                table.CheckConstraint("ck_agents_description_not_blank", "length(btrim(description)) > 0");
                table.CheckConstraint("ck_agents_instructions_valid", "length(btrim(system_instructions)) BETWEEN 1 AND 12000");
                table.CheckConstraint("ck_agents_category_valid", "category IN ('Development','Planning','Quality','Testing','Operations','Documentation','Research','Data','Other')");
                table.CheckConstraint("ck_agents_tags_count", "cardinality(tags) <= 10");
            });

        migrationBuilder.CreateIndex(
            name: "ix_agents_active_created_id",
            table: "agents",
            columns: new[] { "is_active", "created_at", "id" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropTable(name: "agents");
}
