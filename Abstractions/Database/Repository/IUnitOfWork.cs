using CRMService.Abstractions.Database.Repository.Authorization;
using CRMService.Abstractions.Database.Repository.CrmEntity;
using CRMService.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Abstractions.Database.Repository.Report;

namespace CRMService.Abstractions.Database.Repository
{
    public interface IUnitOfWork
    {
        #region [Okdesk Entities]

        ICompanyRepository Company { get; }
        ICompanyCategoryRepository CompanyCategory { get; }
        IParameterRepository Parameter { get; }
        IEmployeeGroupRepository EmployeeGroup { get; }
        IEmployeeRepository Employee { get; }
        IEmployeeRoleRepository EmployeeRole { get; }
        IGroupRepository Group { get; }
        IIssuePriorityRepository IssuePriority { get; }
        IIssueRepository Issue { get; }
        IIssueStatusRepository IssueStatus { get; }
        IIssueTypeRepository IssueType { get; }
        IIssueTypeGroupRepository IssueTypeGroup { get; }
        IKindParamsRepository KindParams { get; }
        IKindRepository Kind { get; }
        IKindParameterRepository KindParameter { get; }
        IMaintenanceEntityRepository MaintenanceEntity { get; }
        IManufacturerRepository Manufacturer { get; }
        IModelRepository Model { get; }
        IOkdeskRoleRepository OkdeskRole { get; }
        ITimeEntryRepository TimeEntry { get; }
        IEquipmentRepository Equipment { get; }

        #endregion

        IReportRepository Report { get; }
        IBlockReasonRepository BlockReason { get; }
        ICrmRoleRepository CrmRole { get; }
        ISessionRepository Session { get; }
        IUserRepository User { get; }
        IUserRoleRepository UserRole { get; }
        IPlanSettingRepository PlanSetting { get; }
        IPlanColorSchemeRepository PlanColor { get; }

        Task SaveChangesAsync(CancellationToken ct = default);
        Task ExecuteInTransaction(Func<Task> action, CancellationToken ct = default);
    }
}