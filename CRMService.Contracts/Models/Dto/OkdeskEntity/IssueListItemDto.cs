namespace CRMService.Contracts.Models.Dto.OkdeskEntity
{
    public class IssueListItemDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public string CompanyCategoryColor { get; set; } = string.Empty;

        public string AssigneeName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string StatusColor { get; set; } = string.Empty;

        public string PriorityName { get; set; } = string.Empty;

        public string PriorityColor { get; set; } = string.Empty;
    }
}
