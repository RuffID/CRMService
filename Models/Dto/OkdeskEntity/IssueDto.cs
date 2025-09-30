namespace CRMService.Models.Dto.OkdeskEntity
{
    public class IssueDto
    {
        public int Id { get; set; }

        public int? AssigneeId { get; set; }

        public int? AuthorId { get; set; }

        public string? Title { get; set; }

        public DateTime? EmployeesUpdatedAt { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? DeadlineAt { get; set; }

        public DateTime? DelayTo { get; set; }

        public DateTime? DeletedAt { get; set; }

        public int? StatusId { get; set; }

        public int? TypeId { get; set; }

        public int? PriorityId { get; set; }

        public int? CompanyId { get; set; }

        public int? ServiceObjectId { get; set; }
    }
}