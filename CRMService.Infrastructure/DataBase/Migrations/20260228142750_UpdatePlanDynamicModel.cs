using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMService.Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlanDynamicModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanSetting",
                table: "PlanSetting");

            migrationBuilder.DropIndex(
                name: "IX_PlanSetting_EmployeeId",
                table: "PlanSetting");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PlanSetting");

            migrationBuilder.DropColumn(
                name: "DayPlan",
                table: "PlanSetting");

            migrationBuilder.RenameColumn(
                name: "MonthPlan",
                table: "PlanSetting",
                newName: "PlanValue");

            migrationBuilder.AddColumn<Guid>(
                name: "PlanId",
                table: "PlanSetting",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "ToPercent",
                table: "PlanColorSchemes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "PlanId",
                table: "PlanColorSchemes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanSetting",
                table: "PlanSetting",
                columns: new[] { "PlanId", "EmployeeId" });

            migrationBuilder.CreateTable(
                name: "GeneralSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanSwitchSeconds = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PlanColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.Sql("""
                DECLARE @DefaultPlanId uniqueidentifier = NEWID();

                INSERT INTO [Plans] ([Id], [Name], [PlanColor])
                VALUES (@DefaultPlanId, N'План по умолчанию', N'#FFC107');

                UPDATE [PlanSetting]
                SET [PlanId] = @DefaultPlanId
                WHERE [PlanId] = '00000000-0000-0000-0000-000000000000';

                UPDATE [PlanColorSchemes]
                SET [PlanId] = @DefaultPlanId
                WHERE [PlanId] = '00000000-0000-0000-0000-000000000000';

                IF NOT EXISTS (SELECT 1 FROM [GeneralSettings])
                BEGIN
                    INSERT INTO [GeneralSettings] ([Id], [PlanSwitchSeconds])
                    VALUES (NEWID(), 10);
                END
                """);


            migrationBuilder.CreateIndex(
                name: "IX_PlanSetting_EmployeeId",
                table: "PlanSetting",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanColorSchemes_PlanId_FromPercent_ToPercent",
                table: "PlanColorSchemes",
                columns: new[] { "PlanId", "FromPercent", "ToPercent" });

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Name",
                table: "Plans",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanColorSchemes_Plans_PlanId",
                table: "PlanColorSchemes",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanSetting_Plans_PlanId",
                table: "PlanSetting",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanColorSchemes_Plans_PlanId",
                table: "PlanColorSchemes");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanSetting_Plans_PlanId",
                table: "PlanSetting");

            migrationBuilder.DropTable(
                name: "GeneralSettings");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanSetting",
                table: "PlanSetting");

            migrationBuilder.DropIndex(
                name: "IX_PlanSetting_EmployeeId",
                table: "PlanSetting");

            migrationBuilder.DropIndex(
                name: "IX_PlanColorSchemes_PlanId_FromPercent_ToPercent",
                table: "PlanColorSchemes");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "PlanSetting");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "PlanColorSchemes");

            migrationBuilder.RenameColumn(
                name: "PlanValue",
                table: "PlanSetting",
                newName: "MonthPlan");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "PlanSetting",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<int>(
                name: "DayPlan",
                table: "PlanSetting",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ToPercent",
                table: "PlanColorSchemes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanSetting",
                table: "PlanSetting",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PlanSetting_EmployeeId",
                table: "PlanSetting",
                column: "EmployeeId",
                unique: true);
        }
    }
}



