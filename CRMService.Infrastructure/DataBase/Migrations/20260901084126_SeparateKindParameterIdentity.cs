using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMService.Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class SeparateKindParameterIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "KindsParameterLocalIdSequence",
                startValue: -1L,
                incrementBy: -1);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "KindsParameters",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR [KindsParameterLocalIdSequence]",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "OkdeskId",
                table: "KindsParameters",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM [KindsParameters]
                    WHERE [Id] = 0
                      AND [Code] <> N'ESM'
                )
                BEGIN
                    THROW 51000, 'Unexpected kind parameter with Id 0. Migration stopped to prevent incorrect data reassignment.', 1;
                END;

                IF EXISTS (
                    SELECT 1
                    FROM [KindsParameters]
                    WHERE [Id] = 0
                      AND [Code] = N'ESM'
                )
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM [KindsParameters]
                        WHERE [Id] = 10503
                          AND [Code] = N'ESM'
                    )
                    BEGIN
                        THROW 51001, 'Canonical ESM kind parameter with Id 10503 was not found.', 1;
                    END;

                    IF EXISTS (
                        SELECT 1
                        FROM [EquipmentParameter] AS [source]
                        INNER JOIN [EquipmentParameter] AS [target]
                            ON [target].[EquipmentId] = [source].[EquipmentId]
                           AND [target].[KindParameterId] = 10503
                        WHERE [source].[KindParameterId] = 0
                    )
                    BEGIN
                        THROW 51002, 'Equipment contains both duplicate and canonical ESM parameter rows. Resolve the conflicting values manually.', 1;
                    END;

                    IF EXISTS (
                        SELECT 1
                        FROM [KindParams] AS [source]
                        INNER JOIN [KindParams] AS [target]
                            ON [target].[KindId] = [source].[KindId]
                           AND [target].[KindParameterId] = 10503
                        WHERE [source].[KindParameterId] = 0
                    )
                    BEGIN
                        THROW 51003, 'Equipment kind contains both duplicate and canonical ESM links. Resolve the conflict manually.', 1;
                    END;

                    UPDATE [EquipmentParameter]
                    SET [KindParameterId] = 10503
                    WHERE [KindParameterId] = 0;

                    UPDATE [KindParams]
                    SET [KindParameterId] = 10503
                    WHERE [KindParameterId] = 0;

                    DELETE FROM [KindsParameters]
                    WHERE [Id] = 0
                      AND [Code] = N'ESM';
                END;

                UPDATE [KindsParameters]
                SET [OkdeskId] = [Id]
                WHERE [Id] > 0;
                """);

            migrationBuilder.CreateIndex(
                name: "UX_KindsParameters_Code",
                table: "KindsParameters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_KindsParameters_OkdeskId",
                table: "KindsParameters",
                column: "OkdeskId",
                unique: true,
                filter: "[OkdeskId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_KindsParameters_Code",
                table: "KindsParameters");

            migrationBuilder.DropIndex(
                name: "UX_KindsParameters_OkdeskId",
                table: "KindsParameters");

            migrationBuilder.DropColumn(
                name: "OkdeskId",
                table: "KindsParameters");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "KindsParameters",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "NEXT VALUE FOR [KindsParameterLocalIdSequence]");

            migrationBuilder.DropSequence(
                name: "KindsParameterLocalIdSequence");
        }
    }
}
