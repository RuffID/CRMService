using CRMService.Interfaces.Entity;
using CRMService.Models.Entity;

namespace CRMService.Models.WebHook
{
    public class IssueJSON : IEntity
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public IssueType? Type { get; set; }
        public IssuePriority? Priority { get; set; }
        public IssueStatus? Status { get; set; }
        public Client? Client { get; set; }
        public MaintenanceEntityWebHook? Maintenance_entity { get; set; }
        public EmployeeWebHook? Author { get; set; }
        public AssigneeWebHook? Assignee { get; set; }
        public DateTime? Created_at { get; set; }
        public DateTime? Deadline_at { get; set; }
        public DateTime? Completed_at { get; set; }

        public Issue ConvertToIssue()
        {
            Issue convertIssue = new();

            convertIssue.Id = Id;
            convertIssue.Title = Title;
            convertIssue.Type = Type;
            convertIssue.TypeId = Type?.Id;
            convertIssue.Priority = Priority;
            convertIssue.PriorityId = Priority?.Id;
            convertIssue.Status = Status;
            convertIssue.StatusId = Status?.Id;
            convertIssue.AuthorId = Author?.Id;
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
