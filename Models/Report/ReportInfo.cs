namespace CRMService.Models.Report
{
    public class ReportInfo
    {
        public long EmployeeId { get; set; }
        public long SolvedIssues { get; set; }
        
        public double SpentedTime { get; set; }

        public IssueInfo[] Issues { get; set; } = [];
    }
}