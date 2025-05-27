using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Report;
using System.Runtime.CompilerServices;

namespace CRMService.Interfaces.Repository
{
    public interface IUnitOfWorkEntities : IDisposable
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
        IKindParamsRepository KindParams { get; }
        IKindRepository Kind { get; }
        IKindParameterRepository KindParameter { get; }
        IMaintenanceEntityRepository MaintenanceEntity { get; }
        IManufacturerRepository Manufacturer { get; }
        IModelRepository Model { get; }
        IRoleRepository Role { get; }
        ITimeEntryRepository TimeEntry { get; }
        IEquipmentRepository Equipment { get; }
        IReportRepository Report { get; }

        Task SaveAsync([CallerMemberName] string caller = "");
    }
}
