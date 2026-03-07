using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMService.Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCompanyCategoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_CompanyCategory_CategoryId",
                table: "Company");

            migrationBuilder.RenameTable(
                name: "CompanyCategory",
                newName: "CompanyCategory_Old");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyCategory",
                table: "CompanyCategory_Old");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyCategory_Old",
                table: "CompanyCategory_Old",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CompanyCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyCategory", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                SET IDENTITY_INSERT [CompanyCategory] ON;

                INSERT INTO [CompanyCategory] ([Id], [Code], [Name], [Color])
                SELECT [Id], [Code], [Name], [Color]
                FROM [CompanyCategory_Old];

                SET IDENTITY_INSERT [CompanyCategory] OFF;
                """);

            migrationBuilder.Sql(
                """
                DECLARE @MAX_COMPANY_CATEGORY_ID INT;
                SELECT @MAX_COMPANY_CATEGORY_ID = MAX([Id]) FROM [CompanyCategory];

                IF @MAX_COMPANY_CATEGORY_ID IS NOT NULL
                BEGIN
                    DBCC CHECKIDENT ('[CompanyCategory]', RESEED, @MAX_COMPANY_CATEGORY_ID);
                END
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Company_CompanyCategory_CategoryId",
                table: "Company",
                column: "CategoryId",
                principalTable: "CompanyCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable(
                name: "CompanyCategory_Old");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_CompanyCategory_CategoryId",
                table: "Company");

            migrationBuilder.RenameTable(
                name: "CompanyCategory",
                newName: "CompanyCategory_New");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyCategory",
                table: "CompanyCategory_New");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyCategory_New",
                table: "CompanyCategory_New",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CompanyCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyCategory", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [CompanyCategory] ([Id], [Code], [Name], [Color])
                SELECT [Id], [Code], [Name], [Color]
                FROM [CompanyCategory_New];
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Company_CompanyCategory_CategoryId",
                table: "Company",
                column: "CategoryId",
                principalTable: "CompanyCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable(
                name: "CompanyCategory_New");
        }
    }
}
