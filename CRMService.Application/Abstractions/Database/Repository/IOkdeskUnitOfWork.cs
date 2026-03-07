using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository
{
    public interface IOkdeskUnitOfWork
    {
        IOkdeskCompanyCategoryRepository CompanyCategory { get; }
        IOkdeskCompanyRepository Company { get; }
        IOkdeskEmployeeRepository Employee { get; }
        IOkdeskGroupRepository Group { get; }
        IOkdeskKindRepository Kind { get; }
        IOkdeskKindParameterRepository KindParameter { get; }
        IOkdeskKindParamsRepository KindParams { get; }
        IOkdeskMaintenanceEntityRepository MaintenanceEntity { get; }
        IOkdeskManufacturerRepository Manufacturer { get; }
        IOkdeskModelRepository Model { get; }
        IOkdeskEquipmentRepository Equipment { get; }
        IOkdeskIssueRepository Issue { get; }
        IOkdeskIssueStatusRepository IssueStatus { get; }
        IOkdeskIssuePriorityRepository IssuePriority { get; }
        IOkdeskIssueTypeRepository IssueType { get; }
        IOkdeskTimeEntryRepository TimeEntry { get; }
    }
}
