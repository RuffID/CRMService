using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMService.Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePriorityAndStatusModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issue_IssuePriority_PriorityId",
                table: "Issue");

            migrationBuilder.DropForeignKey(
                name: "FK_Issue_IssueStatus_StatusId",
                table: "Issue");

            migrationBuilder.RenameTable(
                name: "IssuePriority",
                newName: "IssuePriority_Old");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IssuePriority",
                table: "IssuePriority_Old");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssuePriority_Old",
                table: "IssuePriority_Old",
                column: "Id");

            migrationBuilder.RenameTable(
                name: "IssueStatus",
                newName: "IssueStatus_Old");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IssueStatus",
                table: "IssueStatus_Old");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssueStatus_Old",
                table: "IssueStatus_Old",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "IssuePriority",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuePriority", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IssueStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueStatus", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                SET IDENTITY_INSERT [IssuePriority] ON;

                INSERT INTO [IssuePriority] ([Id], [Name], [Code], [Position], [Color])
                SELECT [Id], [Name], [Code], [Position], [Color]
                FROM [IssuePriority_Old];

                SET IDENTITY_INSERT [IssuePriority] OFF;
                """);

            migrationBuilder.Sql(
                """
                DECLARE @MAX_ISSUE_PRIORITY_ID INT;
                SELECT @MAX_ISSUE_PRIORITY_ID = MAX([Id]) FROM [IssuePriority];

                DBCC CHECKIDENT ('[IssuePriority]', RESEED, @MAX_ISSUE_PRIORITY_ID);
                """);

            migrationBuilder.Sql(
                """
                SET IDENTITY_INSERT [IssueStatus] ON;

                INSERT INTO [IssueStatus] ([Id], [Code], [Name], [Color])
                SELECT [Id], [Code], [Name], [Color]
                FROM [IssueStatus_Old];

                SET IDENTITY_INSERT [IssueStatus] OFF;
                """);

            migrationBuilder.Sql(
                """
                DECLARE @MAX_ISSUE_STATUS_ID INT;
                SELECT @MAX_ISSUE_STATUS_ID = MAX([Id]) FROM [IssueStatus];

                DBCC CHECKIDENT ('[IssueStatus]', RESEED, @MAX_ISSUE_STATUS_ID);
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Issue_IssuePriority_PriorityId",
                table: "Issue",
                column: "PriorityId",
                principalTable: "IssuePriority",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Issue_IssueStatus_StatusId",
                table: "Issue",
                column: "StatusId",
                principalTable: "IssueStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable(
                name: "IssuePriority_Old");

            migrationBuilder.DropTable(
                name: "IssueStatus_Old");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issue_IssuePriority_PriorityId",
                table: "Issue");

            migrationBuilder.DropForeignKey(
                name: "FK_Issue_IssueStatus_StatusId",
                table: "Issue");

            migrationBuilder.RenameTable(
                name: "IssuePriority",
                newName: "IssuePriority_New");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IssuePriority",
                table: "IssuePriority_New");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssuePriority_New",
                table: "IssuePriority_New",
                column: "Id");

            migrationBuilder.RenameTable(
                name: "IssueStatus",
                newName: "IssueStatus_New");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IssueStatus",
                table: "IssueStatus_New");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssueStatus_New",
                table: "IssueStatus_New",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "IssuePriority",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuePriority", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IssueStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueStatus", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [IssuePriority] ([Id], [Name], [Code], [Position], [Color])
                SELECT [Id], [Name], [Code], [Position], [Color]
                FROM [IssuePriority_New];
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [IssueStatus] ([Id], [Code], [Name], [Color])
                SELECT [Id], [Code], [Name], [Color]
                FROM [IssueStatus_New];
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Issue_IssuePriority_PriorityId",
                table: "Issue",
                column: "PriorityId",
                principalTable: "IssuePriority",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Issue_IssueStatus_StatusId",
                table: "Issue",
                column: "StatusId",
                principalTable: "IssueStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable(
                name: "IssuePriority_New");

            migrationBuilder.DropTable(
                name: "IssueStatus_New");
        }
    }
}
