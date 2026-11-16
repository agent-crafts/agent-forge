using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AgentForge.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AgentForgeDbContext))]
[Migration("20261116000000_AddProjectIcon")]
public partial class AddProjectIcon : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "icon", table: "projects", type: "character varying(2048)",
            maxLength: 2048, nullable: false, defaultValue: "");

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "icon", table: "projects");
    }
}
