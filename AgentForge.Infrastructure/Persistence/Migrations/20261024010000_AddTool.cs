using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AgentForge.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AgentForgeDbContext))]
[Migration("20261024010000_AddTool")]
public partial class AddTool : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tools",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                display_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                json_schema = table.Column<string>(type: "jsonb", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_tools", x => x.id));

        migrationBuilder.CreateIndex(
            name: "ix_tools_name", table: "tools", column: "name", unique: true);

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("tools");
    }
}
