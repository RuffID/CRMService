using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMService.Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGroupModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Group_GroupId",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Employee_GroupId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Employee");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Employee",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_GroupId",
                table: "Employee",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Group_GroupId",
                table: "Employee",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id");
        }
    }
}



