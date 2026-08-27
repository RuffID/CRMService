using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CRMService.Infrastructure.IntegrationTests.Database;

[DbContext(typeof(PendingMigrationContext))]
[Migration("20260827000000_TestPendingMigration")]
public class TestPendingMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PendingMigrationMarker",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PendingMigrationMarker", entity => entity.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PendingMigrationMarker");
    }
}
