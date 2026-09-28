using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Models.WebHook
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
        public AssigneeWebHook? New_assignee { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime? Deadline_at { get; set; }
        public DateTime? Completed_at { get; set; }
        public AssigneeWebHook? EffectiveAssignee => New_assignee ?? Assignee;

        public Issue ConvertToIssue() => ConvertToIssue(EffectiveAssignee);

        public Issue ConvertToIssue(AssigneeWebHook? assignee)
        {
            Issue convertIssue = new()
            {
                Id = Id,
                Title = Title,
                Type = Type,
                Priority = Priority,
                Status = Status,
                AuthorId = Author.Id,
                AssigneeId = assignee?.Employee?.Id,
                GroupId = assignee?.Group?.Id,
                CreatedAt = Created_at,
                DeadlineAt = Deadline_at,
                CompletedAt = Completed_at,
                EmployeesUpdatedAt = DateTime.Now,
                Company = Client?.Company
            };

            convertIssue.GroupUpdatedAt = convertIssue.EmployeesUpdatedAt;

            if (Maintenance_entity != null)
                convertIssue.ServiceObject = new MaintenanceEntity() { Id = Maintenance_entity.Id, Name = Maintenance_entity.Name };

            return convertIssue;
        }
    }
}



