using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AgentForge.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AgentForgeDbContext))]
[Migration("20261116000001_RemoveProjectIsArchived")]
public partial class RemoveProjectIsArchived : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "is_archived",
            table: "projects");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_archived",
            table: "projects",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }
}
