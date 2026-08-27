using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.Entity;

namespace CRMService.Infrastructure.DataBase.Repository
{
    public class IssuesUnitOfWork(
        IUnitOfWorkScope scope,
        IIssueRepository issue,
        IIssuePriorityRepository issuePriority,
        IIssueStatusRepository issueStatus,
        IIssueTypeRepository issueType,
        IIssueTypeGroupRepository issueTypeGroup,
        ITimeEntryRepository timeEntry,
        IEmployeeRepository employee,
        ICompanyRepository company,
        IMaintenanceEntityRepository maintenanceEntity) : MainScenarioUnitOfWork(scope), IIssuesUnitOfWork
    {
        public IIssueRepository Issue { get; } = issue;
        public IIssuePriorityRepository IssuePriority { get; } = issuePriority;
        public IIssueStatusRepository IssueStatus { get; } = issueStatus;
        public IIssueTypeRepository IssueType { get; } = issueType;
        public IIssueTypeGroupRepository IssueTypeGroup { get; } = issueTypeGroup;
        public ITimeEntryRepository TimeEntry { get; } = timeEntry;
        public IEmployeeRepository Employee { get; } = employee;
        public ICompanyRepository Company { get; } = company;
        public IMaintenanceEntityRepository MaintenanceEntity { get; } = maintenanceEntity;
    }
}
