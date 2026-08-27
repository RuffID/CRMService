using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.Infrastructure.DataBase.Repository
{
    public class OkdeskIssuesSource(
        IOkdeskEmployeeRepository employee,
        IOkdeskIssueRepository issue,
        IOkdeskIssueStatusRepository issueStatus,
        IOkdeskIssuePriorityRepository issuePriority,
        IOkdeskIssueTypeRepository issueType,
        IOkdeskIssueTypeGroupRepository issueTypeGroup,
        IOkdeskTimeEntryRepository timeEntry) : IOkdeskIssuesSource
    {
        public IOkdeskEmployeeRepository Employee { get; } = employee;
        public IOkdeskIssueRepository Issue { get; } = issue;
        public IOkdeskIssueStatusRepository IssueStatus { get; } = issueStatus;
        public IOkdeskIssuePriorityRepository IssuePriority { get; } = issuePriority;
        public IOkdeskIssueTypeRepository IssueType { get; } = issueType;
        public IOkdeskIssueTypeGroupRepository IssueTypeGroup { get; } = issueTypeGroup;
        public IOkdeskTimeEntryRepository TimeEntry { get; } = timeEntry;
    }
}
