using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AgentForge.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AgentForgeDbContext))]
[Migration("20261110000000_AddAgentTools")]
public partial class AddAgentTools : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "agent_tools",
            columns: table => new
            {
                agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                tool_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_agent_tools", x => new { x.agent_id, x.tool_id });
                table.ForeignKey("fk_agent_tools_agents_agent_id", x => x.agent_id, "agents", "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("fk_agent_tools_tools_tool_id", x => x.tool_id, "tools", "id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex("ix_agent_tools_tool_id", "agent_tools", "tool_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropTable("agent_tools");
}
