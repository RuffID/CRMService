using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.WebHook
{
    public class EventWebHook
    {
        public string? Event_type { get; set; }
        public EmployeeWebHook? Author { get; set; }
        public IssueStatus? New_status { get; set; }
        public IssueType? New_type { get; set; }
        public TimeEntryWebHook[]? Time_entries { get; set; }
        public AssigneeWebHook? New_Assignee { get; set; }
        public CommentWebHook? Comment { get; set; }
        public IssuePriority? Old_priority { get; set; }
        public IssuePriority? New_priority { get; set; }
    }
}
