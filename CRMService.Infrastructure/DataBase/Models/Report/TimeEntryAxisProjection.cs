namespace CRMService.Infrastructure.DataBase.Models.Report
{
    public class TimeEntryAxisProjection
    {
        public int EmployeeId { get; set; }

        public DateTime TimeValue { get; set; }

        public double SpentedTime { get; set; }
    }
}