using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AgentForge.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AgentForgeDbContext))]
[Migration("20261116000000_NormalizeToolCategories")]
public partial class NormalizeToolCategories : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Categories are persisted as strings. Preserve Development; place retired
        // categories in Discovery for explicit reclassification by the user.
        migrationBuilder.Sql("""
            UPDATE tools
            SET category = 'Discovery'
            WHERE category IN ('General', 'Integration', 'Data', 'Automation');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The previous category of each row cannot be reconstructed safely.
    }
}
