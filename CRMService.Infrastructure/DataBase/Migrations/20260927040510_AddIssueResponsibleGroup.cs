using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMService.Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueResponsibleGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Issue",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "issue_groupId_idx",
                table: "Issue",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "issue_groupId_idx",
                table: "Issue");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Issue");
        }
    }
}
