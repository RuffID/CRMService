using CRMService.Models.Entity;

namespace CRMService.Models.WebHook
{
    public class Event
    {
        public string? Event_type { get; set; }
        public EmployeeWebHook? Author { get; set; }
        public IssueStatus? New_status { get; set; }
        public IssueType? New_type { get; set; }
        public TimeEntryWebHook[]? Time_entries { get; set; }
        public AssigneeWebHook? New_Assignee { get; set; }
        public Comment? Comment { get; set; }
    }
}
