using CRMService.Interfaces.Entity;
using Newtonsoft.Json;

namespace CRMService.Models.Entity
{
    public class Issue : IEntity
    {
        public int Id { get; set; }

        public int? AssigneeId { get; set; }

        public int? AuthorId { get; set; }

        public string? Title { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? EmployeesUpdatedAt { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [JsonProperty("deadline_at")]
        public DateTime? DeadlineAt { get; set; }

        [JsonProperty("delay_to")]
        public DateTime? DelayTo { get; set; }

        [JsonProperty("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        public int? StatusId { get; set; }

        public int? TypeId { get; set; }

        public int? PriorityId { get; set; }

        public int? CompanyId { get; set; }

        public int? ServiceObjectId { get; set; }

        public virtual Employee? Assignee { get; set; }

        public virtual Company? Company { get; set; }

        public virtual IssuePriority? Priority { get; set; }

        [JsonProperty("service_object")]
        public virtual MaintenanceEntity? ServiceObject { get; set; }

        public virtual IssueStatus? Status { get; set; }

        public virtual IssueType? Type { get; set; }

        public virtual ICollection<TimeEntry>? TimeEntries { get; set; } = [];

        public void CopyData(Issue item)
        {
            if (item.AuthorId != null)
                AuthorId = item.AuthorId;           
            if (item.StatusId != null)
                StatusId = item.StatusId;
            if (item.PriorityId != null)
                PriorityId = item.PriorityId;
            if (item.TypeId != null)
                TypeId = item.TypeId;

            AssigneeId = item.AssigneeId;
            CompanyId = item.CompanyId;
            ServiceObjectId = item.ServiceObjectId;
            Title = item.Title;
            EmployeesUpdatedAt = item.EmployeesUpdatedAt;
            CreatedAt = item.CreatedAt;
            DeadlineAt = item.DeadlineAt;
            DelayTo = item.DelayTo;
            DeletedAt = item.DeletedAt;
            CompletedAt = item.CompletedAt;
        }
    }
}