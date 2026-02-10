namespace CRMService.Models.Report
{
    public class IssueInfo
    {
        public int Id { get; set; }
        public int? StatusId { get; set; }
        public int? PriorityId { get; set; }
        public int? TypeId { get; set; }
        public int EmployeeId { get; set; }
    }
}