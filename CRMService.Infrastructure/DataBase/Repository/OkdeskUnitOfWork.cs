using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.Infrastructure.DataBase.Repository
{
    public class OkdeskUnitOfWork(
        IOkdeskCompanyCategoryRepository companyCategory,
        IOkdeskCompanyRepository company,
        IOkdeskEmployeeRepository employee,
        IOkdeskGroupRepository group,
        IOkdeskKindRepository kind,
        IOkdeskKindParameterRepository kindParameter,
        IOkdeskKindParamsRepository kindParams,
        IOkdeskMaintenanceEntityRepository maintenanceEntity,
        IOkdeskManufacturerRepository manufacturer,
        IOkdeskModelRepository model,
        IOkdeskEquipmentRepository equipment,
        IOkdeskIssueRepository issue,
        IOkdeskIssueStatusRepository issueStatus,
        IOkdeskIssuePriorityRepository issuePriority,
        IOkdeskIssueTypeRepository issueType,
        IOkdeskIssueTypeGroupRepository issueTypeGroup,
        IOkdeskTimeEntryRepository timeEntry) : IOkdeskUnitOfWork
    {
        public IOkdeskCompanyCategoryRepository CompanyCategory { get; } = companyCategory;
        public IOkdeskCompanyRepository Company { get; } = company;
        public IOkdeskEmployeeRepository Employee { get; } = employee;
        public IOkdeskGroupRepository Group { get; } = group;
        public IOkdeskKindRepository Kind { get; } = kind;
        public IOkdeskKindParameterRepository KindParameter { get; } = kindParameter;
        public IOkdeskKindParamsRepository KindParams { get; } = kindParams;
        public IOkdeskMaintenanceEntityRepository MaintenanceEntity { get; } = maintenanceEntity;
        public IOkdeskManufacturerRepository Manufacturer { get; } = manufacturer;
        public IOkdeskModelRepository Model { get; } = model;
        public IOkdeskEquipmentRepository Equipment { get; } = equipment;
        public IOkdeskIssueRepository Issue { get; } = issue;
        public IOkdeskIssueStatusRepository IssueStatus { get; } = issueStatus;
        public IOkdeskIssuePriorityRepository IssuePriority { get; } = issuePriority;
        public IOkdeskIssueTypeRepository IssueType { get; } = issueType;
        public IOkdeskIssueTypeGroupRepository IssueTypeGroup { get; set; } = issueTypeGroup;
        public IOkdeskTimeEntryRepository TimeEntry { get; } = timeEntry;
    }
}