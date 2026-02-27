using CRMService.Abstractions.Database.Repository;
using CRMService.Abstractions.Database.Repository.Authorization;
using CRMService.Abstractions.Database.Repository.CrmEntity;
using CRMService.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Abstractions.Database.Repository.Report;
using EFCoreLibrary.Abstractions.Database;
using Microsoft.EntityFrameworkCore.Storage;

namespace CRMService.DataBase.Repository
{
    public class UnitOfWork(IAppDbContext context, 
        ICompanyRepository company, 
        ICompanyCategoryRepository companyCategory, 
        IParameterRepository parameter, 
        IEmployeeGroupRepository employeeGroup, 
        IEmployeeRepository employee, 
        IEmployeeRoleRepository employeeRole, 
        IGroupRepository group, 
        IIssuePriorityRepository issuePriority, 
        IIssueRepository issue, 
        IIssueStatusRepository issueStatus, 
        IIssueTypeRepository issueType, 
        IIssueTypeGroupRepository issueTypeGroup,
        IKindParamsRepository kindParams, 
        IKindRepository kind, 
        IKindParameterRepository kindParameter, 
        IMaintenanceEntityRepository maintenanceEntity, 
        IManufacturerRepository manufacturer, 
        IModelRepository model, 
        IOkdeskRoleRepository okdeskRole, 
        ITimeEntryRepository timeEntry, 
        IEquipmentRepository equipment, 
        IReportRepository report, 
        IBlockReasonRepository blockReason, 
        ICrmRoleRepository crmRole, 
        ISessionRepository session, 
        IUserRepository user, 
        IUserRoleRepository userRole,
        IPlanSettingRepository planSetting,
        IPlanColorSchemeRepository planColor) : IUnitOfWork
    {
        public ICompanyRepository Company { get; } = company;
        public ICompanyCategoryRepository CompanyCategory { get; } = companyCategory;
        public IParameterRepository Parameter { get; } = parameter;
        public IEmployeeGroupRepository EmployeeGroup { get; } = employeeGroup;
        public IEmployeeRepository Employee { get; } = employee;
        public IEmployeeRoleRepository EmployeeRole { get; } = employeeRole;
        public IGroupRepository Group { get; } = group;
        public IIssuePriorityRepository IssuePriority { get; } = issuePriority;
        public IIssueRepository Issue { get; } = issue;
        public IIssueStatusRepository IssueStatus { get; } = issueStatus;
        public IIssueTypeRepository IssueType { get; } = issueType;
        public IIssueTypeGroupRepository IssueTypeGroup { get; } = issueTypeGroup;
        public IKindParamsRepository KindParams { get; } = kindParams;
        public IKindRepository Kind { get; } = kind;
        public IKindParameterRepository KindParameter { get; } = kindParameter;
        public IMaintenanceEntityRepository MaintenanceEntity { get; } = maintenanceEntity;
        public IManufacturerRepository Manufacturer { get; } = manufacturer;
        public IModelRepository Model { get; } = model;
        public IOkdeskRoleRepository OkdeskRole { get; } = okdeskRole;
        public ITimeEntryRepository TimeEntry { get; } = timeEntry;
        public IEquipmentRepository Equipment { get; } = equipment;

        public IReportRepository Report { get; } = report;
        public IBlockReasonRepository BlockReason { get; } = blockReason;
        public ICrmRoleRepository CrmRole { get; } = crmRole;
        public ISessionRepository Session { get; } = session;
        public IUserRepository User { get; } = user;
        public IUserRoleRepository UserRole { get; } = userRole;
        public IPlanSettingRepository PlanSetting { get; } = planSetting;
        public IPlanColorSchemeRepository PlanColor { get; set; } = planColor;

        public Task SaveChangesAsync(CancellationToken ct = default) => context.SaveChanges(ct);

        public async Task ExecuteInTransaction(Func<Task> action, CancellationToken ct = default)
        {
            await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                await action();
                await SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }
}
