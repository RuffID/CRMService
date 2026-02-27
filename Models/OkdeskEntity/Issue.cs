using EFCoreLibrary.Abstractions.Entity;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace CRMService.Models.OkdeskEntity
{
    public class Issue : IEntity<int>, ICopyable<Issue>
    {
        public int Id { get; set; }

        public int? AssigneeId { get; set; }

        public int? AuthorId { get; set; }

        public string Title { get; set; } = string.Empty;

        [JsonProperty("updated_at")]
        public DateTime EmployeesUpdatedAt { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

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

        [JsonProperty("service_object")]
        public virtual MaintenanceEntity? ServiceObject { get; set; }

        public virtual IssuePriority? Priority { get; set; }

        public virtual IssueStatus? Status { get; set; }

        public virtual IssueType? Type { get; set; }

        public virtual ICollection<TimeEntry>? TimeEntries { get; set; } = new List<TimeEntry>();

        public void CopyData(Issue item)
        {
            AssigneeId = item.AssigneeId;
            AuthorId = item.AuthorId;
            Title = item.Title;
            EmployeesUpdatedAt = item.EmployeesUpdatedAt;
            CreatedAt = item.CreatedAt;
            CompletedAt = item.CompletedAt;
            DeadlineAt = item.DeadlineAt;
            DelayTo = item.DelayTo;
            DeletedAt = item.DeletedAt;
            StatusId = item.StatusId;
            TypeId = item.TypeId;
            PriorityId = item.PriorityId;
            CompanyId = item.CompanyId;
            ServiceObjectId = item.ServiceObjectId;
        }
    }
}