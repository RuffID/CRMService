namespace CRMService.Application.Models.Report
{
    public class ReportInfo
    {
        public int EmployeeId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Patronymic { get; set; }
        public int SolvedIssues { get; set; }     
        public double SpentedTime { get; set; }

        public Guid? PlanId { get; set; }
        public int? PlanValue { get; set; }
        public string? PlanColor { get; set; }

        public List<IssueInfo> Issues { get; set; } = new ();
    }
}


