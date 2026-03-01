using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMService.Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "employee",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_employee_GroupId",
                table: "employee",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_employee_group_GroupId",
                table: "employee",
                column: "GroupId",
                principalTable: "group",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employee_group_GroupId",
                table: "employee");

            migrationBuilder.DropIndex(
                name: "IX_employee_GroupId",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "employee");
        }
    }
}



