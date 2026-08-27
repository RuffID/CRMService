using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository
{
    public interface IOkdeskIssuesSource
    {
        IOkdeskEmployeeRepository Employee { get; }
        IOkdeskIssueRepository Issue { get; }
        IOkdeskIssueStatusRepository IssueStatus { get; }
        IOkdeskIssuePriorityRepository IssuePriority { get; }
        IOkdeskIssueTypeRepository IssueType { get; }
        IOkdeskIssueTypeGroupRepository IssueTypeGroup { get; }
        IOkdeskTimeEntryRepository TimeEntry { get; }
    }
}
