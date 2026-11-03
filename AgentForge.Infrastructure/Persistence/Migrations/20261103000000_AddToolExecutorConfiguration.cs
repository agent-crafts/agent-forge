using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AgentForge.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AgentForgeDbContext))]
[Migration("20261103000000_AddToolExecutorConfiguration")]
public partial class AddToolExecutorConfiguration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "executor_key", table: "tools", type: "character varying(155)", maxLength: 155, nullable: true);
        migrationBuilder.AddColumn<string>(name: "configuration_json", table: "tools", type: "jsonb", nullable: false, defaultValue: "{}");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "executor_key", table: "tools");
        migrationBuilder.DropColumn(name: "configuration_json", table: "tools");
    }
}
