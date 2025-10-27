using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.WebHook
{
    public class IssueWebHook
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public IssueType Type { get; set; } = null!;
        public IssuePriority Priority { get; set; } = null!;
        public IssueStatus Status { get; set; } = null!;
        public ClientWebHook? Client { get; set; }
        public MaintenanceEntityWebHook? Maintenance_entity { get; set; }
        public EmployeeWebHook Author { get; set; } = null!;
        public AssigneeWebHook? Assignee { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime? Deadline_at { get; set; }
        public DateTime? Completed_at { get; set; }

        public Issue ConvertToIssue()
        {
            Issue convertIssue = new();

            convertIssue.Id = Id;
            convertIssue.Title = Title;
            convertIssue.Type = Type;
            convertIssue.TypeId = Type.Id;
            convertIssue.Priority = Priority;
            convertIssue.PriorityId = Priority.Id;
            convertIssue.Status = Status;
            convertIssue.StatusId = Status.Id;
            convertIssue.AuthorId = Author.Id;
            if (Assignee != null && Assignee.Employee != null)
            {
                convertIssue.Assignee = new() { Id = Assignee.Employee.Id };
            }
            convertIssue.AssigneeId = Assignee?.Employee?.Id;
            convertIssue.CreatedAt = Created_at;
            convertIssue.DeadlineAt = Deadline_at;
            convertIssue.CompletedAt = Completed_at;
            convertIssue.EmployeesUpdatedAt = DateTime.Now;
            convertIssue.Company = Client?.Company;
            if (Maintenance_entity != null)
                convertIssue.ServiceObject = new MaintenanceEntity() { Id = Maintenance_entity.Id, Name = Maintenance_entity.Name };

            return convertIssue;
        }
    }
}
