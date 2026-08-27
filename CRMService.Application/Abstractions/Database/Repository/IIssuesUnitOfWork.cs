using CRMService.Application.Abstractions.Database.Repository.Entity;

namespace CRMService.Application.Abstractions.Database.Repository
{
    public interface IIssuesUnitOfWork : IUnitOfWorkScope
    {
        IIssueRepository Issue { get; }
        IIssuePriorityRepository IssuePriority { get; }
        IIssueStatusRepository IssueStatus { get; }
        IIssueTypeRepository IssueType { get; }
        IIssueTypeGroupRepository IssueTypeGroup { get; }
        ITimeEntryRepository TimeEntry { get; }
        IEmployeeRepository Employee { get; }
        ICompanyRepository Company { get; }
        IMaintenanceEntityRepository MaintenanceEntity { get; }
    }
}
