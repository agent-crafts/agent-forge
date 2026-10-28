using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AgentForge.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AgentForgeDbContext))]
[Migration("20261027010000_AllowEmptyAgentInstructions")]
public partial class AllowEmptyAgentInstructions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_agents_instructions_valid",
            table: "agents");
        migrationBuilder.AddCheckConstraint(
            name: "ck_agents_instructions_valid",
            table: "agents",
            sql: "length(btrim(system_instructions)) <= 12000");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_agents_instructions_valid",
            table: "agents");
        migrationBuilder.AddCheckConstraint(
            name: "ck_agents_instructions_valid",
            table: "agents",
            sql: "length(btrim(system_instructions)) BETWEEN 1 AND 12000");
    }
}
