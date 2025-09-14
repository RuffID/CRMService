using CRMService.Interfaces.Repository.Authorization;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Report;

namespace CRMService.Interfaces.Repository
{
    public interface IUnitOfWork
    {
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
        IReportRepository Report { get; }
        IBlockReasonRepository BlockReason { get; }
        ICrmRoleRepository CrmRole { get; }
        ISessionRepository Session { get; }
        IUserRepository User { get; }
        IUserRoleRepository UserRole { get; }

        Task SaveAsync(CancellationToken ct = default);
        Task ExecuteInTransaction(Func<Task> action, CancellationToken ct = default);
    }
}