namespace CRMService.Contracts.Models.Dto.OkdeskEntity
{
    public class IssueDetailsDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public string CompanyCategoryColor { get; set; } = string.Empty;

        public string ServiceObjectName { get; set; } = string.Empty;

        public string AssigneeName { get; set; } = string.Empty;

        public string AuthorName { get; set; } = string.Empty;

        public string TypeName { get; set; } = string.Empty;

        public string StatusName { get; set; } = string.Empty;

        public string StatusColor { get; set; } = string.Empty;

        public string PriorityName { get; set; } = string.Empty;

        public string PriorityColor { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? DeadlineAt { get; set; }

        public DateTime? DelayTo { get; set; }
    }
}
