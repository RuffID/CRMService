using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMService.Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueResponsibleGroupVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GroupUpdatedAt",
                table: "Issue",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [Issue]
                SET [GroupUpdatedAt] = [EmployeesUpdatedAt]
                WHERE [GroupId] IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupUpdatedAt",
                table: "Issue");
        }
    }
}
