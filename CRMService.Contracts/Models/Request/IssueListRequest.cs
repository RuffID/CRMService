namespace CRMService.Contracts.Models.Request
{
    public class IssueListRequest
    {
        public int? NumberFrom { get; set; }

        public int? NumberTo { get; set; }

        public string? Search { get; set; }

        public List<int>? AssigneeIds { get; set; }

        public List<int>? AuthorIds { get; set; }

        public List<int>? TypeIds { get; set; }

        public List<int>? StatusIds { get; set; }

        public List<int>? PriorityIds { get; set; }

        public List<int>? CompanyIds { get; set; }

        public List<int>? GroupIds { get; set; }

        public DateTime? RegistrationDateFrom { get; set; }

        public DateTime? RegistrationDateTo { get; set; }

        public DateTime? ResolutionDateFrom { get; set; }

        public DateTime? ResolutionDateTo { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
