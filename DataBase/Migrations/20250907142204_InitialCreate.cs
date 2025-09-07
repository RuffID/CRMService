using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMService.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company_category",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    color = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_role",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    name = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crm_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "group",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "issue_priority",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    code = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    position = table.Column<int>(type: "int", nullable: true),
                    color = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_priority", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "issue_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    code = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    name = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    color = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "issue_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    @default = table.Column<bool>(name: "default", type: "bit", nullable: true),
                    inner = table.Column<bool>(type: "bit", nullable: true),
                    available_for_client = table.Column<bool>(type: "bit", nullable: true),
                    type = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kind",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    visible = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kind", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kinds_parameters",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    fieldType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kinds_parameters", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "manufacturers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    description = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    visible = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manufacturers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "okdesk_role",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_okdesk_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    login = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    additional_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    categoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company", x => x.id);
                    table.ForeignKey(
                        name: "FK_company_company_category_categoryId",
                        column: x => x.categoryId,
                        principalTable: "company_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "employee",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    last_name = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    patronymic = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    position = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    login = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "group",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "kindparams",
                columns: table => new
                {
                    kindId = table.Column<int>(type: "int", nullable: false),
                    kindParameterId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kindparams", x => new { x.kindId, x.kindParameterId });
                    table.ForeignKey(
                        name: "kindParameters",
                        column: x => x.kindParameterId,
                        principalTable: "kinds_parameters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "kindPrm",
                        column: x => x.kindId,
                        principalTable: "kind",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "model",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    visible = table.Column<bool>(type: "bit", nullable: false),
                    kindId = table.Column<int>(type: "int", nullable: false),
                    manufacturerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model", x => x.id);
                    table.ForeignKey(
                        name: "kindId",
                        column: x => x.kindId,
                        principalTable: "kind",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "manufacturerId",
                        column: x => x.manufacturerId,
                        principalTable: "manufacturers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "block_reason",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    blocking_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    unblocking_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    reason_block = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    reason_unblock = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_block_reason", x => x.id);
                    table.ForeignKey(
                        name: "user_block_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    refresh_token = table.Column<string>(type: "nvarchar(88)", maxLength: 88, nullable: false),
                    expiration_refresh_token = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session", x => x.id);
                    table.ForeignKey(
                        name: "user_session_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    role_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_role", x => new { x.role_id, x.user_id });
                    table.ForeignKey(
                        name: "role_id",
                        column: x => x.role_id,
                        principalTable: "crm_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_entity",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    companyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_entity", x => x.id);
                    table.ForeignKey(
                        name: "companyId",
                        column: x => x.companyId,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_groups",
                columns: table => new
                {
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    groupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_groups", x => new { x.employeeId, x.groupId });
                    table.ForeignKey(
                        name: "FK_employee_groups_employee_employeeId",
                        column: x => x.employeeId,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employee_groups_group_groupId",
                        column: x => x.groupId,
                        principalTable: "group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_roles",
                columns: table => new
                {
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    roleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_roles", x => new { x.employeeId, x.roleId });
                    table.ForeignKey(
                        name: "FK_employee_roles_employee_employeeId",
                        column: x => x.employeeId,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employee_roles_okdesk_role_roleId",
                        column: x => x.roleId,
                        principalTable: "okdesk_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equipment",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    serial_number = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    inventory_number = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    kindId = table.Column<int>(type: "int", nullable: true),
                    manufacturerId = table.Column<int>(type: "int", nullable: true),
                    modelId = table.Column<int>(type: "int", nullable: true),
                    companyId = table.Column<int>(type: "int", nullable: true),
                    maintenanceEntitiesId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment", x => x.id);
                    table.ForeignKey(
                        name: "FK_equipment_model_modelId",
                        column: x => x.modelId,
                        principalTable: "model",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "equipmentCompanyId",
                        column: x => x.companyId,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "equipmentMaintenanceEntitiesId",
                        column: x => x.maintenanceEntitiesId,
                        principalTable: "maintenance_entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "kindEquipId",
                        column: x => x.kindId,
                        principalTable: "kind",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "manufacturerEquipId",
                        column: x => x.manufacturerId,
                        principalTable: "manufacturers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "issue",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    assignee_id = table.Column<int>(type: "int", nullable: true),
                    author_id = table.Column<int>(type: "int", nullable: true),
                    title = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    employees_updated_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    deadline_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    delay_to = table.Column<DateTime>(type: "datetime", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    statusId = table.Column<int>(type: "int", nullable: false),
                    typeId = table.Column<int>(type: "int", nullable: false),
                    priorityId = table.Column<int>(type: "int", nullable: false),
                    companyId = table.Column<int>(type: "int", nullable: true),
                    service_objectId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue", x => x.id);
                    table.ForeignKey(
                        name: "issue_assigneeId",
                        column: x => x.assignee_id,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "issue_companyId",
                        column: x => x.companyId,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "issue_priorityId",
                        column: x => x.priorityId,
                        principalTable: "issue_priority",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "issue_service_objectId",
                        column: x => x.service_objectId,
                        principalTable: "maintenance_entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "issue_statusId",
                        column: x => x.statusId,
                        principalTable: "issue_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "issue_typeId",
                        column: x => x.typeId,
                        principalTable: "issue_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "equipment_parameter",
                columns: table => new
                {
                    equipmentId = table.Column<int>(type: "int", nullable: false),
                    kindParameterId = table.Column<int>(type: "int", nullable: false),
                    value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_parameter", x => new { x.equipmentId, x.kindParameterId });
                    table.ForeignKey(
                        name: "FK_equipment_parameter_equipment_equipmentId",
                        column: x => x.equipmentId,
                        principalTable: "equipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_equipment_parameter_kinds_parameters_kindParameterId",
                        column: x => x.kindParameterId,
                        principalTable: "kinds_parameters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "time_entry",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    spentTime = table.Column<double>(type: "float", nullable: false),
                    issueId = table.Column<int>(type: "int", nullable: false),
                    logged_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_entry", x => x.id);
                    table.ForeignKey(
                        name: "employeeId",
                        column: x => x.employeeId,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "issueId",
                        column: x => x.issueId,
                        principalTable: "issue",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_block_reason_user_id",
                table: "block_reason",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "categoryId_idx",
                table: "company",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_GroupId",
                table: "employee",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_groups_groupId",
                table: "employee_groups",
                column: "groupId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_roles_roleId",
                table: "employee_roles",
                column: "roleId");

            migrationBuilder.CreateIndex(
                name: "companyId_idx",
                table: "equipment",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "kindId_idx",
                table: "equipment",
                column: "kindId");

            migrationBuilder.CreateIndex(
                name: "maintenanceEntitiesId_idx",
                table: "equipment",
                column: "maintenanceEntitiesId");

            migrationBuilder.CreateIndex(
                name: "manufacturerEquipId_idx",
                table: "equipment",
                column: "manufacturerId");

            migrationBuilder.CreateIndex(
                name: "modelEquipId_idx",
                table: "equipment",
                column: "modelId");

            migrationBuilder.CreateIndex(
                name: "equipmentId_idx",
                table: "equipment_parameter",
                column: "equipmentId");

            migrationBuilder.CreateIndex(
                name: "kindParameterId_idx",
                table: "equipment_parameter",
                column: "kindParameterId");

            migrationBuilder.CreateIndex(
                name: "issue_assigneeId_idx",
                table: "issue",
                column: "assignee_id");

            migrationBuilder.CreateIndex(
                name: "issue_companyId_idx",
                table: "issue",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "issue_priorityId_idx",
                table: "issue",
                column: "priorityId");

            migrationBuilder.CreateIndex(
                name: "issue_service_objectId_idx",
                table: "issue",
                column: "service_objectId");

            migrationBuilder.CreateIndex(
                name: "issue_statusId_idx",
                table: "issue",
                column: "statusId");

            migrationBuilder.CreateIndex(
                name: "issue_typeId_idx",
                table: "issue",
                column: "typeId");

            migrationBuilder.CreateIndex(
                name: "kindKey_idx",
                table: "kindparams",
                column: "kindId");

            migrationBuilder.CreateIndex(
                name: "kindParameters_idx",
                table: "kindparams",
                column: "kindParameterId");

            migrationBuilder.CreateIndex(
                name: "companyId_idx",
                table: "maintenance_entity",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "kindId_idx",
                table: "model",
                column: "kindId");

            migrationBuilder.CreateIndex(
                name: "manufacturerId_idx",
                table: "model",
                column: "manufacturerId");

            migrationBuilder.CreateIndex(
                name: "user_session_id_idx",
                table: "session",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "employeeId_idx",
                table: "time_entry",
                column: "employeeId");

            migrationBuilder.CreateIndex(
                name: "issueId_idx",
                table: "time_entry",
                column: "issueId");

            migrationBuilder.CreateIndex(
                name: "login_UNIQUE",
                table: "user",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_role_user_id",
                table: "user_role",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "block_reason");

            migrationBuilder.DropTable(
                name: "employee_groups");

            migrationBuilder.DropTable(
                name: "employee_roles");

            migrationBuilder.DropTable(
                name: "equipment_parameter");

            migrationBuilder.DropTable(
                name: "kindparams");

            migrationBuilder.DropTable(
                name: "session");

            migrationBuilder.DropTable(
                name: "time_entry");

            migrationBuilder.DropTable(
                name: "user_role");

            migrationBuilder.DropTable(
                name: "okdesk_role");

            migrationBuilder.DropTable(
                name: "equipment");

            migrationBuilder.DropTable(
                name: "kinds_parameters");

            migrationBuilder.DropTable(
                name: "issue");

            migrationBuilder.DropTable(
                name: "crm_role");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "model");

            migrationBuilder.DropTable(
                name: "employee");

            migrationBuilder.DropTable(
                name: "issue_priority");

            migrationBuilder.DropTable(
                name: "maintenance_entity");

            migrationBuilder.DropTable(
                name: "issue_status");

            migrationBuilder.DropTable(
                name: "issue_type");

            migrationBuilder.DropTable(
                name: "kind");

            migrationBuilder.DropTable(
                name: "manufacturers");

            migrationBuilder.DropTable(
                name: "group");

            migrationBuilder.DropTable(
                name: "company");

            migrationBuilder.DropTable(
                name: "company_category");
        }
    }
}
