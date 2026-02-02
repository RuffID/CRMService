using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMService.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_block_id",
                table: "block_reason");

            migrationBuilder.DropForeignKey(
                name: "FK_company_company_category_categoryId",
                table: "company");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_group_GroupId",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_groups_employee_employeeId",
                table: "employee_groups");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_groups_group_groupId",
                table: "employee_groups");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_roles_employee_employeeId",
                table: "employee_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_roles_okdesk_role_roleId",
                table: "employee_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_equipment_model_modelId",
                table: "equipment");

            migrationBuilder.DropForeignKey(
                name: "equipmentCompanyId",
                table: "equipment");

            migrationBuilder.DropForeignKey(
                name: "equipmentMaintenanceEntitiesId",
                table: "equipment");

            migrationBuilder.DropForeignKey(
                name: "kindEquipId",
                table: "equipment");

            migrationBuilder.DropForeignKey(
                name: "manufacturerEquipId",
                table: "equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_equipment_parameter_equipment_equipmentId",
                table: "equipment_parameter");

            migrationBuilder.DropForeignKey(
                name: "FK_equipment_parameter_kinds_parameters_kindParameterId",
                table: "equipment_parameter");

            migrationBuilder.DropForeignKey(
                name: "issue_assigneeId",
                table: "issue");

            migrationBuilder.DropForeignKey(
                name: "issue_companyId",
                table: "issue");

            migrationBuilder.DropForeignKey(
                name: "issue_priorityId",
                table: "issue");

            migrationBuilder.DropForeignKey(
                name: "issue_service_objectId",
                table: "issue");

            migrationBuilder.DropForeignKey(
                name: "issue_statusId",
                table: "issue");

            migrationBuilder.DropForeignKey(
                name: "issue_typeId",
                table: "issue");

            migrationBuilder.DropForeignKey(
                name: "FK_issue_type_issue_type_groups_groupId",
                table: "issue_type");

            migrationBuilder.DropForeignKey(
                name: "FK_issue_type_groups_issue_type_groups_parent_group_id",
                table: "issue_type_groups");

            migrationBuilder.DropForeignKey(
                name: "kindParameters",
                table: "kindparams");

            migrationBuilder.DropForeignKey(
                name: "kindPrm",
                table: "kindparams");

            migrationBuilder.DropForeignKey(
                name: "companyId",
                table: "maintenance_entity");

            migrationBuilder.DropForeignKey(
                name: "kindId",
                table: "model");

            migrationBuilder.DropForeignKey(
                name: "manufacturerId",
                table: "model");

            migrationBuilder.DropForeignKey(
                name: "user_session_id",
                table: "session");

            migrationBuilder.DropForeignKey(
                name: "employeeId",
                table: "time_entry");

            migrationBuilder.DropForeignKey(
                name: "issueId",
                table: "time_entry");

            migrationBuilder.DropForeignKey(
                name: "role_id",
                table: "user_role");

            migrationBuilder.DropForeignKey(
                name: "user_id",
                table: "user_role");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user",
                table: "user");

            migrationBuilder.DropPrimaryKey(
                name: "PK_session",
                table: "session");

            migrationBuilder.DropPrimaryKey(
                name: "PK_model",
                table: "model");

            migrationBuilder.DropPrimaryKey(
                name: "PK_manufacturers",
                table: "manufacturers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_kindparams",
                table: "kindparams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_kind",
                table: "kind");

            migrationBuilder.DropPrimaryKey(
                name: "PK_issue",
                table: "issue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_group",
                table: "group");

            migrationBuilder.DropPrimaryKey(
                name: "PK_equipment",
                table: "equipment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employee",
                table: "employee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_company",
                table: "company");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_role",
                table: "user_role");

            migrationBuilder.DropPrimaryKey(
                name: "PK_time_entry",
                table: "time_entry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_okdesk_role",
                table: "okdesk_role");

            migrationBuilder.DropPrimaryKey(
                name: "PK_maintenance_entity",
                table: "maintenance_entity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_kinds_parameters",
                table: "kinds_parameters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_issue_type_groups",
                table: "issue_type_groups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_issue_type",
                table: "issue_type");

            migrationBuilder.DropPrimaryKey(
                name: "PK_issue_status",
                table: "issue_status");

            migrationBuilder.DropPrimaryKey(
                name: "PK_issue_priority",
                table: "issue_priority");

            migrationBuilder.DropPrimaryKey(
                name: "PK_equipment_parameter",
                table: "equipment_parameter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employee_roles",
                table: "employee_roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employee_groups",
                table: "employee_groups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_crm_role",
                table: "crm_role");

            migrationBuilder.DropPrimaryKey(
                name: "PK_company_category",
                table: "company_category");

            migrationBuilder.DropPrimaryKey(
                name: "PK_block_reason",
                table: "block_reason");

            migrationBuilder.RenameTable(
                name: "user",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "session",
                newName: "Session");

            migrationBuilder.RenameTable(
                name: "model",
                newName: "Model");

            migrationBuilder.RenameTable(
                name: "manufacturers",
                newName: "Manufacturers");

            migrationBuilder.RenameTable(
                name: "kindparams",
                newName: "KindParams");

            migrationBuilder.RenameTable(
                name: "kind",
                newName: "Kind");

            migrationBuilder.RenameTable(
                name: "issue",
                newName: "Issue");

            migrationBuilder.RenameTable(
                name: "group",
                newName: "Group");

            migrationBuilder.RenameTable(
                name: "equipment",
                newName: "Equipment");

            migrationBuilder.RenameTable(
                name: "employee",
                newName: "Employee");

            migrationBuilder.RenameTable(
                name: "company",
                newName: "Company");

            migrationBuilder.RenameTable(
                name: "user_role",
                newName: "UserRole");

            migrationBuilder.RenameTable(
                name: "time_entry",
                newName: "TimeEntry");

            migrationBuilder.RenameTable(
                name: "okdesk_role",
                newName: "OkdeskRole");

            migrationBuilder.RenameTable(
                name: "maintenance_entity",
                newName: "MaintenanceEntity");

            migrationBuilder.RenameTable(
                name: "kinds_parameters",
                newName: "KindsParameters");

            migrationBuilder.RenameTable(
                name: "issue_type_groups",
                newName: "IssueTypeGroups");

            migrationBuilder.RenameTable(
                name: "issue_type",
                newName: "IssueType");

            migrationBuilder.RenameTable(
                name: "issue_status",
                newName: "IssueStatus");

            migrationBuilder.RenameTable(
                name: "issue_priority",
                newName: "IssuePriority");

            migrationBuilder.RenameTable(
                name: "equipment_parameter",
                newName: "EquipmentParameter");

            migrationBuilder.RenameTable(
                name: "employee_roles",
                newName: "EmployeeRoles");

            migrationBuilder.RenameTable(
                name: "employee_groups",
                newName: "EmployeeGroups");

            migrationBuilder.RenameTable(
                name: "crm_role",
                newName: "CrmRole");

            migrationBuilder.RenameTable(
                name: "company_category",
                newName: "CompanyCategory");

            migrationBuilder.RenameTable(
                name: "block_reason",
                newName: "BlockReason");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "User",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "login",
                table: "User",
                newName: "Login");

            migrationBuilder.RenameColumn(
                name: "active",
                table: "User",
                newName: "Active");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "User",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "User",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Session",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Session",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "refresh_token",
                table: "Session",
                newName: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "expiration_refresh_token",
                table: "Session",
                newName: "ExpirationRefreshToken");

            migrationBuilder.RenameColumn(
                name: "visible",
                table: "Model",
                newName: "Visible");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Model",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "manufacturerId",
                table: "Model",
                newName: "ManufacturerId");

            migrationBuilder.RenameColumn(
                name: "kindId",
                table: "Model",
                newName: "KindId");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Model",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "Model",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Model",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "visible",
                table: "Manufacturers",
                newName: "Visible");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Manufacturers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Manufacturers",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "Manufacturers",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Manufacturers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "kindParameterId",
                table: "KindParams",
                newName: "KindParameterId");

            migrationBuilder.RenameColumn(
                name: "kindId",
                table: "KindParams",
                newName: "KindId");

            migrationBuilder.RenameColumn(
                name: "visible",
                table: "Kind",
                newName: "Visible");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Kind",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Kind",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "Kind",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Kind",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "typeId",
                table: "Issue",
                newName: "TypeId");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Issue",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "statusId",
                table: "Issue",
                newName: "StatusId");

            migrationBuilder.RenameColumn(
                name: "priorityId",
                table: "Issue",
                newName: "PriorityId");

            migrationBuilder.RenameColumn(
                name: "companyId",
                table: "Issue",
                newName: "CompanyId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Issue",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "service_objectId",
                table: "Issue",
                newName: "ServiceObjectId");

            migrationBuilder.RenameColumn(
                name: "employees_updated_at",
                table: "Issue",
                newName: "EmployeesUpdatedAt");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "Issue",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "delay_to",
                table: "Issue",
                newName: "DelayTo");

            migrationBuilder.RenameColumn(
                name: "deadline_at",
                table: "Issue",
                newName: "DeadlineAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Issue",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "completed_at",
                table: "Issue",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "author_id",
                table: "Issue",
                newName: "AuthorId");

            migrationBuilder.RenameColumn(
                name: "assignee_id",
                table: "Issue",
                newName: "AssigneeId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Group",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Group",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "active",
                table: "Group",
                newName: "Active");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Group",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "modelId",
                table: "Equipment",
                newName: "ModelId");

            migrationBuilder.RenameColumn(
                name: "manufacturerId",
                table: "Equipment",
                newName: "ManufacturerId");

            migrationBuilder.RenameColumn(
                name: "maintenanceEntitiesId",
                table: "Equipment",
                newName: "MaintenanceEntitiesId");

            migrationBuilder.RenameColumn(
                name: "kindId",
                table: "Equipment",
                newName: "KindId");

            migrationBuilder.RenameColumn(
                name: "companyId",
                table: "Equipment",
                newName: "CompanyId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Equipment",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "serial_number",
                table: "Equipment",
                newName: "SerialNumber");

            migrationBuilder.RenameColumn(
                name: "inventory_number",
                table: "Equipment",
                newName: "InventoryNumber");

            migrationBuilder.RenameColumn(
                name: "position",
                table: "Employee",
                newName: "Position");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "Employee",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "patronymic",
                table: "Employee",
                newName: "Patronymic");

            migrationBuilder.RenameColumn(
                name: "login",
                table: "Employee",
                newName: "Login");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Employee",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "active",
                table: "Employee",
                newName: "Active");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Employee",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "Employee",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "Employee",
                newName: "FirstName");

            migrationBuilder.RenameIndex(
                name: "IX_employee_GroupId",
                table: "Employee",
                newName: "IX_Employee_GroupId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Company",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "categoryId",
                table: "Company",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "active",
                table: "Company",
                newName: "Active");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Company",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "additional_name",
                table: "Company",
                newName: "AdditionalName");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "UserRole",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "UserRole",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_user_role_user_id",
                table: "UserRole",
                newName: "IX_UserRole_UserId");

            migrationBuilder.RenameColumn(
                name: "spentTime",
                table: "TimeEntry",
                newName: "SpentTime");

            migrationBuilder.RenameColumn(
                name: "issueId",
                table: "TimeEntry",
                newName: "IssueId");

            migrationBuilder.RenameColumn(
                name: "employeeId",
                table: "TimeEntry",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "TimeEntry",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "logged_at",
                table: "TimeEntry",
                newName: "LoggedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "TimeEntry",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "OkdeskRole",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OkdeskRole",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "MaintenanceEntity",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "companyId",
                table: "MaintenanceEntity",
                newName: "CompanyId");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "MaintenanceEntity",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "active",
                table: "MaintenanceEntity",
                newName: "Active");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MaintenanceEntity",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "KindsParameters",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "fieldType",
                table: "KindsParameters",
                newName: "FieldType");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "KindsParameters",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "KindsParameters",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "IssueTypeGroups",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "IssueTypeGroups",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "IssueTypeGroups",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "parent_group_id",
                table: "IssueTypeGroups",
                newName: "ParentGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_issue_type_groups_parent_group_id",
                table: "IssueTypeGroups",
                newName: "IX_IssueTypeGroups_ParentGroupId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "IssueType",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "groupId",
                table: "IssueType",
                newName: "GroupId");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "IssueType",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "IssueType",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "is_inner",
                table: "IssueType",
                newName: "IsInner");

            migrationBuilder.RenameColumn(
                name: "is_default",
                table: "IssueType",
                newName: "IsDefault");

            migrationBuilder.RenameColumn(
                name: "available_for_client",
                table: "IssueType",
                newName: "AvailableForClient");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "IssueStatus",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "color",
                table: "IssueStatus",
                newName: "Color");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "IssueStatus",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "IssueStatus",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "position",
                table: "IssuePriority",
                newName: "Position");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "IssuePriority",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "color",
                table: "IssuePriority",
                newName: "Color");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "IssuePriority",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "IssuePriority",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "EquipmentParameter",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "kindParameterId",
                table: "EquipmentParameter",
                newName: "KindParameterId");

            migrationBuilder.RenameColumn(
                name: "equipmentId",
                table: "EquipmentParameter",
                newName: "EquipmentId");

            migrationBuilder.RenameColumn(
                name: "roleId",
                table: "EmployeeRoles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "employeeId",
                table: "EmployeeRoles",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_employee_roles_roleId",
                table: "EmployeeRoles",
                newName: "IX_EmployeeRoles_RoleId");

            migrationBuilder.RenameColumn(
                name: "groupId",
                table: "EmployeeGroups",
                newName: "GroupId");

            migrationBuilder.RenameColumn(
                name: "employeeId",
                table: "EmployeeGroups",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_employee_groups_groupId",
                table: "EmployeeGroups",
                newName: "IX_EmployeeGroups_GroupId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "CrmRole",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CrmRole",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "CompanyCategory",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "color",
                table: "CompanyCategory",
                newName: "Color");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "CompanyCategory",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CompanyCategory",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "BlockReason",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "BlockReason",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "unblocking_date",
                table: "BlockReason",
                newName: "UnblockingDate");

            migrationBuilder.RenameColumn(
                name: "reason_unblock",
                table: "BlockReason",
                newName: "UnblockingReason");

            migrationBuilder.RenameColumn(
                name: "reason_block",
                table: "BlockReason",
                newName: "ReasonBlock");

            migrationBuilder.RenameColumn(
                name: "blocking_date",
                table: "BlockReason",
                newName: "BlockingDate");

            migrationBuilder.RenameIndex(
                name: "IX_block_reason_user_id",
                table: "BlockReason",
                newName: "IX_BlockReason_UserId");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "User",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Session",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpirationRefreshToken",
                table: "Session",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EmployeesUpdatedAt",
                table: "Issue",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "Issue",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DelayTo",
                table: "Issue",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeadlineAt",
                table: "Issue",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Issue",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "Issue",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LoggedAt",
                table: "TimeEntry",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TimeEntry",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CrmRole",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "BlockReason",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UnblockingDate",
                table: "BlockReason",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "BlockingDate",
                table: "BlockReason",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Session",
                table: "Session",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Model",
                table: "Model",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Manufacturers",
                table: "Manufacturers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KindParams",
                table: "KindParams",
                columns: new[] { "KindId", "KindParameterId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kind",
                table: "Kind",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Issue",
                table: "Issue",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Group",
                table: "Group",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Equipment",
                table: "Equipment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employee",
                table: "Employee",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Company",
                table: "Company",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRole",
                table: "UserRole",
                columns: new[] { "RoleId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeEntry",
                table: "TimeEntry",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OkdeskRole",
                table: "OkdeskRole",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MaintenanceEntity",
                table: "MaintenanceEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KindsParameters",
                table: "KindsParameters",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssueTypeGroups",
                table: "IssueTypeGroups",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssueType",
                table: "IssueType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssueStatus",
                table: "IssueStatus",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssuePriority",
                table: "IssuePriority",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EquipmentParameter",
                table: "EquipmentParameter",
                columns: new[] { "EquipmentId", "KindParameterId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeRoles",
                table: "EmployeeRoles",
                columns: new[] { "EmployeeId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeGroups",
                table: "EmployeeGroups",
                columns: new[] { "EmployeeId", "GroupId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CrmRole",
                table: "CrmRole",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyCategory",
                table: "CompanyCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlockReason",
                table: "BlockReason",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlockReason_User_UserId",
                table: "BlockReason",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Company_CompanyCategory_CategoryId",
                table: "Company",
                column: "CategoryId",
                principalTable: "CompanyCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Group_GroupId",
                table: "Employee",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeGroups_Employee_EmployeeId",
                table: "EmployeeGroups",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeGroups_Group_GroupId",
                table: "EmployeeGroups",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRoles_Employee_EmployeeId",
                table: "EmployeeRoles",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRoles_OkdeskRole_RoleId",
                table: "EmployeeRoles",
                column: "RoleId",
                principalTable: "OkdeskRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipment_Company_CompanyId",
                table: "Equipment",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipment_Kind_KindId",
                table: "Equipment",
                column: "KindId",
                principalTable: "Kind",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipment_MaintenanceEntity_MaintenanceEntitiesId",
                table: "Equipment",
                column: "MaintenanceEntitiesId",
                principalTable: "MaintenanceEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipment_Manufacturers_ManufacturerId",
                table: "Equipment",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipment_Model_ModelId",
                table: "Equipment",
                column: "ModelId",
                principalTable: "Model",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentParameter_Equipment_EquipmentId",
                table: "EquipmentParameter",
                column: "EquipmentId",
                principalTable: "Equipment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentParameter_KindsParameters_KindParameterId",
                table: "EquipmentParameter",
                column: "KindParameterId",
                principalTable: "KindsParameters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Issue_Company_CompanyId",
                table: "Issue",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Issue_Employee_AssigneeId",
                table: "Issue",
                column: "AssigneeId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Issue_IssueType_TypeId",
                table: "Issue",
                column: "TypeId",
                principalTable: "IssueType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Issue_MaintenanceEntity_ServiceObjectId",
                table: "Issue",
                column: "ServiceObjectId",
                principalTable: "MaintenanceEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueType_IssueTypeGroups_GroupId",
                table: "IssueType",
                column: "GroupId",
                principalTable: "IssueTypeGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueTypeGroups_IssueTypeGroups_ParentGroupId",
                table: "IssueTypeGroups",
                column: "ParentGroupId",
                principalTable: "IssueTypeGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KindParams_Kind_KindId",
                table: "KindParams",
                column: "KindId",
                principalTable: "Kind",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KindParams_KindsParameters_KindParameterId",
                table: "KindParams",
                column: "KindParameterId",
                principalTable: "KindsParameters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceEntity_Company_CompanyId",
                table: "MaintenanceEntity",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Model_Kind_KindId",
                table: "Model",
                column: "KindId",
                principalTable: "Kind",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Model_Manufacturers_ManufacturerId",
                table: "Model",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_User_UserId",
                table: "Session",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntry_Employee_EmployeeId",
                table: "TimeEntry",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntry_Issue_IssueId",
                table: "TimeEntry",
                column: "IssueId",
                principalTable: "Issue",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_CrmRole_RoleId",
                table: "UserRole",
                column: "RoleId",
                principalTable: "CrmRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_User_UserId",
                table: "UserRole",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlockReason_User_UserId",
                table: "BlockReason");

            migrationBuilder.DropForeignKey(
                name: "FK_Company_CompanyCategory_CategoryId",
                table: "Company");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Group_GroupId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeGroups_Employee_EmployeeId",
                table: "EmployeeGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeGroups_Group_GroupId",
                table: "EmployeeGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRoles_Employee_EmployeeId",
                table: "EmployeeRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRoles_OkdeskRole_RoleId",
                table: "EmployeeRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipment_Company_CompanyId",
                table: "Equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipment_Kind_KindId",
                table: "Equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipment_MaintenanceEntity_MaintenanceEntitiesId",
                table: "Equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipment_Manufacturers_ManufacturerId",
                table: "Equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipment_Model_ModelId",
                table: "Equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentParameter_Equipment_EquipmentId",
                table: "EquipmentParameter");

            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentParameter_KindsParameters_KindParameterId",
                table: "EquipmentParameter");

            migrationBuilder.DropForeignKey(
                name: "FK_Issue_Company_CompanyId",
                table: "Issue");

            migrationBuilder.DropForeignKey(
                name: "FK_Issue_Employee_AssigneeId",
                table: "Issue");

            migrationBuilder.DropForeignKey(
                name: "FK_Issue_IssuePriority_PriorityId",
                table: "Issue");

            migrationBuilder.DropForeignKey(
                name: "FK_Issue_IssueStatus_StatusId",
                table: "Issue");

            migrationBuilder.DropForeignKey(
                name: "FK_Issue_IssueType_TypeId",
                table: "Issue");

            migrationBuilder.DropForeignKey(
                name: "FK_Issue_MaintenanceEntity_ServiceObjectId",
                table: "Issue");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueType_IssueTypeGroups_GroupId",
                table: "IssueType");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueTypeGroups_IssueTypeGroups_ParentGroupId",
                table: "IssueTypeGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_KindParams_Kind_KindId",
                table: "KindParams");

            migrationBuilder.DropForeignKey(
                name: "FK_KindParams_KindsParameters_KindParameterId",
                table: "KindParams");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceEntity_Company_CompanyId",
                table: "MaintenanceEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_Model_Kind_KindId",
                table: "Model");

            migrationBuilder.DropForeignKey(
                name: "FK_Model_Manufacturers_ManufacturerId",
                table: "Model");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_User_UserId",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntry_Employee_EmployeeId",
                table: "TimeEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntry_Issue_IssueId",
                table: "TimeEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_CrmRole_RoleId",
                table: "UserRole");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_User_UserId",
                table: "UserRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Session",
                table: "Session");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Model",
                table: "Model");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Manufacturers",
                table: "Manufacturers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KindParams",
                table: "KindParams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kind",
                table: "Kind");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Issue",
                table: "Issue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Group",
                table: "Group");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Equipment",
                table: "Equipment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employee",
                table: "Employee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Company",
                table: "Company");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRole",
                table: "UserRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeEntry",
                table: "TimeEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OkdeskRole",
                table: "OkdeskRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MaintenanceEntity",
                table: "MaintenanceEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KindsParameters",
                table: "KindsParameters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IssueTypeGroups",
                table: "IssueTypeGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IssueType",
                table: "IssueType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IssueStatus",
                table: "IssueStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IssuePriority",
                table: "IssuePriority");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EquipmentParameter",
                table: "EquipmentParameter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeRoles",
                table: "EmployeeRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeGroups",
                table: "EmployeeGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CrmRole",
                table: "CrmRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyCategory",
                table: "CompanyCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlockReason",
                table: "BlockReason");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "user");

            migrationBuilder.RenameTable(
                name: "Session",
                newName: "session");

            migrationBuilder.RenameTable(
                name: "Model",
                newName: "model");

            migrationBuilder.RenameTable(
                name: "Manufacturers",
                newName: "manufacturers");

            migrationBuilder.RenameTable(
                name: "KindParams",
                newName: "kindparams");

            migrationBuilder.RenameTable(
                name: "Kind",
                newName: "kind");

            migrationBuilder.RenameTable(
                name: "Issue",
                newName: "issue");

            migrationBuilder.RenameTable(
                name: "Group",
                newName: "group");

            migrationBuilder.RenameTable(
                name: "Equipment",
                newName: "equipment");

            migrationBuilder.RenameTable(
                name: "Employee",
                newName: "employee");

            migrationBuilder.RenameTable(
                name: "Company",
                newName: "company");

            migrationBuilder.RenameTable(
                name: "UserRole",
                newName: "user_role");

            migrationBuilder.RenameTable(
                name: "TimeEntry",
                newName: "time_entry");

            migrationBuilder.RenameTable(
                name: "OkdeskRole",
                newName: "okdesk_role");

            migrationBuilder.RenameTable(
                name: "MaintenanceEntity",
                newName: "maintenance_entity");

            migrationBuilder.RenameTable(
                name: "KindsParameters",
                newName: "kinds_parameters");

            migrationBuilder.RenameTable(
                name: "IssueTypeGroups",
                newName: "issue_type_groups");

            migrationBuilder.RenameTable(
                name: "IssueType",
                newName: "issue_type");

            migrationBuilder.RenameTable(
                name: "IssueStatus",
                newName: "issue_status");

            migrationBuilder.RenameTable(
                name: "IssuePriority",
                newName: "issue_priority");

            migrationBuilder.RenameTable(
                name: "EquipmentParameter",
                newName: "equipment_parameter");

            migrationBuilder.RenameTable(
                name: "EmployeeRoles",
                newName: "employee_roles");

            migrationBuilder.RenameTable(
                name: "EmployeeGroups",
                newName: "employee_groups");

            migrationBuilder.RenameTable(
                name: "CrmRole",
                newName: "crm_role");

            migrationBuilder.RenameTable(
                name: "CompanyCategory",
                newName: "company_category");

            migrationBuilder.RenameTable(
                name: "BlockReason",
                newName: "block_reason");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "user",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Login",
                table: "user",
                newName: "login");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "user",
                newName: "active");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "user",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "user",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "session",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "session",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "session",
                newName: "refresh_token");

            migrationBuilder.RenameColumn(
                name: "ExpirationRefreshToken",
                table: "session",
                newName: "expiration_refresh_token");

            migrationBuilder.RenameColumn(
                name: "Visible",
                table: "model",
                newName: "visible");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "model",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "ManufacturerId",
                table: "model",
                newName: "manufacturerId");

            migrationBuilder.RenameColumn(
                name: "KindId",
                table: "model",
                newName: "kindId");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "model",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "model",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "model",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Visible",
                table: "manufacturers",
                newName: "visible");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "manufacturers",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "manufacturers",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "manufacturers",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "manufacturers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "KindParameterId",
                table: "kindparams",
                newName: "kindParameterId");

            migrationBuilder.RenameColumn(
                name: "KindId",
                table: "kindparams",
                newName: "kindId");

            migrationBuilder.RenameColumn(
                name: "Visible",
                table: "kind",
                newName: "visible");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "kind",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "kind",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "kind",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "kind",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "issue",
                newName: "typeId");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "issue",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "issue",
                newName: "statusId");

            migrationBuilder.RenameColumn(
                name: "PriorityId",
                table: "issue",
                newName: "priorityId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "issue",
                newName: "companyId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "issue",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ServiceObjectId",
                table: "issue",
                newName: "service_objectId");

            migrationBuilder.RenameColumn(
                name: "EmployeesUpdatedAt",
                table: "issue",
                newName: "employees_updated_at");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "issue",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "DelayTo",
                table: "issue",
                newName: "delay_to");

            migrationBuilder.RenameColumn(
                name: "DeadlineAt",
                table: "issue",
                newName: "deadline_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "issue",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "issue",
                newName: "completed_at");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "issue",
                newName: "author_id");

            migrationBuilder.RenameColumn(
                name: "AssigneeId",
                table: "issue",
                newName: "assignee_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "group",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "group",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "group",
                newName: "active");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "group",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ModelId",
                table: "equipment",
                newName: "modelId");

            migrationBuilder.RenameColumn(
                name: "ManufacturerId",
                table: "equipment",
                newName: "manufacturerId");

            migrationBuilder.RenameColumn(
                name: "MaintenanceEntitiesId",
                table: "equipment",
                newName: "maintenanceEntitiesId");

            migrationBuilder.RenameColumn(
                name: "KindId",
                table: "equipment",
                newName: "kindId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "equipment",
                newName: "companyId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "equipment",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SerialNumber",
                table: "equipment",
                newName: "serial_number");

            migrationBuilder.RenameColumn(
                name: "InventoryNumber",
                table: "equipment",
                newName: "inventory_number");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "employee",
                newName: "position");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "employee",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Patronymic",
                table: "employee",
                newName: "patronymic");

            migrationBuilder.RenameColumn(
                name: "Login",
                table: "employee",
                newName: "login");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "employee",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "employee",
                newName: "active");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "employee",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "employee",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "employee",
                newName: "first_name");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_GroupId",
                table: "employee",
                newName: "IX_employee_GroupId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "company",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "company",
                newName: "categoryId");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "company",
                newName: "active");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "company",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "AdditionalName",
                table: "company",
                newName: "additional_name");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_role",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "user_role",
                newName: "role_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserRole_UserId",
                table: "user_role",
                newName: "IX_user_role_user_id");

            migrationBuilder.RenameColumn(
                name: "SpentTime",
                table: "time_entry",
                newName: "spentTime");

            migrationBuilder.RenameColumn(
                name: "IssueId",
                table: "time_entry",
                newName: "issueId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "time_entry",
                newName: "employeeId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "time_entry",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LoggedAt",
                table: "time_entry",
                newName: "logged_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "time_entry",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "okdesk_role",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "okdesk_role",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "maintenance_entity",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "maintenance_entity",
                newName: "companyId");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "maintenance_entity",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "maintenance_entity",
                newName: "active");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "maintenance_entity",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "kinds_parameters",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "FieldType",
                table: "kinds_parameters",
                newName: "fieldType");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "kinds_parameters",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "kinds_parameters",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "issue_type_groups",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "issue_type_groups",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "issue_type_groups",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ParentGroupId",
                table: "issue_type_groups",
                newName: "parent_group_id");

            migrationBuilder.RenameIndex(
                name: "IX_IssueTypeGroups_ParentGroupId",
                table: "issue_type_groups",
                newName: "IX_issue_type_groups_parent_group_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "issue_type",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "issue_type",
                newName: "groupId");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "issue_type",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "issue_type",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "IsInner",
                table: "issue_type",
                newName: "is_inner");

            migrationBuilder.RenameColumn(
                name: "IsDefault",
                table: "issue_type",
                newName: "is_default");

            migrationBuilder.RenameColumn(
                name: "AvailableForClient",
                table: "issue_type",
                newName: "available_for_client");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "issue_status",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Color",
                table: "issue_status",
                newName: "color");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "issue_status",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "issue_status",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "issue_priority",
                newName: "position");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "issue_priority",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Color",
                table: "issue_priority",
                newName: "color");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "issue_priority",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "issue_priority",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "equipment_parameter",
                newName: "value");

            migrationBuilder.RenameColumn(
                name: "KindParameterId",
                table: "equipment_parameter",
                newName: "kindParameterId");

            migrationBuilder.RenameColumn(
                name: "EquipmentId",
                table: "equipment_parameter",
                newName: "equipmentId");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "employee_roles",
                newName: "roleId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "employee_roles",
                newName: "employeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeRoles_RoleId",
                table: "employee_roles",
                newName: "IX_employee_roles_roleId");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "employee_groups",
                newName: "groupId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "employee_groups",
                newName: "employeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeGroups_GroupId",
                table: "employee_groups",
                newName: "IX_employee_groups_groupId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "crm_role",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "crm_role",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "company_category",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Color",
                table: "company_category",
                newName: "color");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "company_category",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "company_category",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "block_reason",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "block_reason",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UnblockingReason",
                table: "block_reason",
                newName: "reason_unblock");

            migrationBuilder.RenameColumn(
                name: "UnblockingDate",
                table: "block_reason",
                newName: "unblocking_date");

            migrationBuilder.RenameColumn(
                name: "ReasonBlock",
                table: "block_reason",
                newName: "reason_block");

            migrationBuilder.RenameColumn(
                name: "BlockingDate",
                table: "block_reason",
                newName: "blocking_date");

            migrationBuilder.RenameIndex(
                name: "IX_BlockReason_UserId",
                table: "block_reason",
                newName: "IX_block_reason_user_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "user",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "session",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "expiration_refresh_token",
                table: "session",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "employees_updated_at",
                table: "issue",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_at",
                table: "issue",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "delay_to",
                table: "issue",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "deadline_at",
                table: "issue",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "issue",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "completed_at",
                table: "issue",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "logged_at",
                table: "time_entry",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "time_entry",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "crm_role",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "block_reason",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "unblocking_date",
                table: "block_reason",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "blocking_date",
                table: "block_reason",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user",
                table: "user",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_session",
                table: "session",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_model",
                table: "model",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_manufacturers",
                table: "manufacturers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_kindparams",
                table: "kindparams",
                columns: new[] { "kindId", "kindParameterId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_kind",
                table: "kind",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_issue",
                table: "issue",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_group",
                table: "group",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_equipment",
                table: "equipment",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_employee",
                table: "employee",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_company",
                table: "company",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_role",
                table: "user_role",
                columns: new[] { "role_id", "user_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_time_entry",
                table: "time_entry",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_okdesk_role",
                table: "okdesk_role",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_maintenance_entity",
                table: "maintenance_entity",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_kinds_parameters",
                table: "kinds_parameters",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_issue_type_groups",
                table: "issue_type_groups",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_issue_type",
                table: "issue_type",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_issue_status",
                table: "issue_status",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_issue_priority",
                table: "issue_priority",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_equipment_parameter",
                table: "equipment_parameter",
                columns: new[] { "equipmentId", "kindParameterId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_employee_roles",
                table: "employee_roles",
                columns: new[] { "employeeId", "roleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_employee_groups",
                table: "employee_groups",
                columns: new[] { "employeeId", "groupId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_crm_role",
                table: "crm_role",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_company_category",
                table: "company_category",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_block_reason",
                table: "block_reason",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "user_block_id",
                table: "block_reason",
                column: "user_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_company_company_category_categoryId",
                table: "company",
                column: "categoryId",
                principalTable: "company_category",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_group_GroupId",
                table: "employee",
                column: "GroupId",
                principalTable: "group",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_employee_groups_employee_employeeId",
                table: "employee_groups",
                column: "employeeId",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_groups_group_groupId",
                table: "employee_groups",
                column: "groupId",
                principalTable: "group",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_roles_employee_employeeId",
                table: "employee_roles",
                column: "employeeId",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_roles_okdesk_role_roleId",
                table: "employee_roles",
                column: "roleId",
                principalTable: "okdesk_role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_equipment_model_modelId",
                table: "equipment",
                column: "modelId",
                principalTable: "model",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "equipmentCompanyId",
                table: "equipment",
                column: "companyId",
                principalTable: "company",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "equipmentMaintenanceEntitiesId",
                table: "equipment",
                column: "maintenanceEntitiesId",
                principalTable: "maintenance_entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "kindEquipId",
                table: "equipment",
                column: "kindId",
                principalTable: "kind",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "manufacturerEquipId",
                table: "equipment",
                column: "manufacturerId",
                principalTable: "manufacturers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_equipment_parameter_equipment_equipmentId",
                table: "equipment_parameter",
                column: "equipmentId",
                principalTable: "equipment",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_equipment_parameter_kinds_parameters_kindParameterId",
                table: "equipment_parameter",
                column: "kindParameterId",
                principalTable: "kinds_parameters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "issue_assigneeId",
                table: "issue",
                column: "assignee_id",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "issue_companyId",
                table: "issue",
                column: "companyId",
                principalTable: "company",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "issue_priorityId",
                table: "issue",
                column: "priorityId",
                principalTable: "issue_priority",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "issue_service_objectId",
                table: "issue",
                column: "service_objectId",
                principalTable: "maintenance_entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "issue_statusId",
                table: "issue",
                column: "statusId",
                principalTable: "issue_status",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "issue_typeId",
                table: "issue",
                column: "typeId",
                principalTable: "issue_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_issue_type_issue_type_groups_groupId",
                table: "issue_type",
                column: "groupId",
                principalTable: "issue_type_groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_issue_type_groups_issue_type_groups_parent_group_id",
                table: "issue_type_groups",
                column: "parent_group_id",
                principalTable: "issue_type_groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "kindParameters",
                table: "kindparams",
                column: "kindParameterId",
                principalTable: "kinds_parameters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "kindPrm",
                table: "kindparams",
                column: "kindId",
                principalTable: "kind",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "companyId",
                table: "maintenance_entity",
                column: "companyId",
                principalTable: "company",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "kindId",
                table: "model",
                column: "kindId",
                principalTable: "kind",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "manufacturerId",
                table: "model",
                column: "manufacturerId",
                principalTable: "manufacturers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "user_session_id",
                table: "session",
                column: "user_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "employeeId",
                table: "time_entry",
                column: "employeeId",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "issueId",
                table: "time_entry",
                column: "issueId",
                principalTable: "issue",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "role_id",
                table: "user_role",
                column: "role_id",
                principalTable: "crm_role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "user_id",
                table: "user_role",
                column: "user_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
